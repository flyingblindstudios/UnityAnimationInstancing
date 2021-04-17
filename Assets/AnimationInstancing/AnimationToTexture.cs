using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class AnimationToTexture : MonoBehaviour
{
    [SerializeField] GameObject Character = null;
    [SerializeField] AnimationClip[] Clips = null;
    [SerializeField] string[] Slots = null;
    [SerializeField] string RootBoneName = "Hips";

    private List<Transform> Bones = new List<Transform>();
    private List<Matrix4x4> InverseBind = new List<Matrix4x4>();
    private void Start()
    {
        StartCoroutine(Convert("TestAnimationData"));
    }

    int GetIndex(int x, int y, int width)
    {
        return y* width + x;
    }

    private IEnumerator Convert(string FileNameOut)
    {
        GameObject tmpObj = Instantiate(Character);

        //because of rotation i need 360
        float SampleRat = 1f/30f;

        float totalAnimationTime = 0;
        foreach (AnimationClip clip in Clips)
        {
            if(totalAnimationTime < clip.length)
            { 
                totalAnimationTime = clip.length;
            }
        }

        Debug.Log("Total animation secs: " + totalAnimationTime);

        Transform rootBone = tmpObj.transform.Find(RootBoneName);


        SkinnedMeshRenderer skinned = tmpObj.GetComponentInChildren<SkinnedMeshRenderer>();
        Bones.AddRange(skinned.bones);

        Debug.Log("Number of Bones: " + Bones.Count);

        for (int b = 0; b < Bones.Count; b++)
        {
            Matrix4x4 iMatrix = skinned.sharedMesh.bindposes[b];
            InverseBind.Add(iMatrix);   
        }

        int TextureSizeY = Bones.Count;
        int TextureSizeX = Mathf.CeilToInt(totalAnimationTime / SampleRat) * 4/*Matrix of rgba * 4?*/;

        TextureAnimationData TextureAnimationData = ScriptableObject.CreateInstance<TextureAnimationData>();


        Texture2D texture = new Texture2D(TextureSizeX, TextureSizeY, TextureFormat.RGBAFloat, false, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float normalizationFactor = 1.0f/ 360.0f;
    

        float totalD = 0;
        //sample animations
        int AnimationCount = 0;
        foreach (AnimationClip clip in Clips)
        {
            TextureAnimationData.AnimationClip clipData = new TextureAnimationData.AnimationClip();
            clipData.StartFrame = 0;
            clipData.EndFrame = Mathf.FloorToInt(((float)TextureSizeX)*0.25f * clip.length/ totalAnimationTime);
            clipData.Length = clip.length;
            clipData.AnimationIndex = AnimationCount;
            clipData.Name = clip.name;
            TextureAnimationData.AnimationClips.Add(clipData);

            //add slots
            clipData.AnimationSlots.Clear();

            //samples we take for this animationclip
            int numAnimationSamples = Mathf.CeilToInt(clipData.Length / SampleRat);

            for (int i = 0; i < Slots.Length; i++)
            {
                TextureAnimationData.AnimationSlot slot = new TextureAnimationData.AnimationSlot();
                slot.Skinning = new Matrix4x4[numAnimationSamples];
                slot.Pos = new Vector3[numAnimationSamples];
                slot.Rot = new Quaternion[numAnimationSamples];
                slot.Name = Slots[i];
                clipData.AnimationSlots.Add(slot);
            }
           
           

            int xPos = 0;
            int animSamplePos = 0;
            for (float d = 0; d < clip.length; d += SampleRat)
            {
                clip.SampleAnimation(tmpObj, d);
                for (int b = 0; b < Bones.Count; b++)
                {
                    int yPos = b + AnimationCount * Bones.Count;

                    //rootbone worldtolocal to remove root movement
                    Matrix4x4 matrix = /*rootBone.worldToLocalMatrix **/ Bones[b].localToWorldMatrix * InverseBind[b];


                    //normalize
                    Vector4 r0 = matrix.GetRow(0) * normalizationFactor;
                    Vector4 r1 = matrix.GetRow(1) * normalizationFactor;
                    Vector4 r2 = matrix.GetRow(2) * normalizationFactor;
                    Vector4 r3 = matrix.GetRow(3) * normalizationFactor;
                    Vector4 offset4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    Vector3 offset3 = new Vector3(1.0f, 1.0f, 1.0f);
                    //offset because negative values
                    r0 = (r0 + offset4) * 0.5f;
                    r1 = (r1 + offset4) * 0.5f;
                    r2 = (r2 + offset4) * 0.5f;
                    r3 = (r3 + offset4) * 0.5f;

                    texture.SetPixel(xPos, yPos, r0);
                    texture.SetPixel(xPos + 1, yPos, r1);
                    texture.SetPixel(xPos + 2, yPos, r2);
                    texture.SetPixel(xPos + 3, yPos, r3);

                    //go through slots
                    for (int i = 0; i < clipData.AnimationSlots.Count; i++)
                    {
                        //is it the bone we need for the slot??
                        if (string.Compare(clipData.AnimationSlots[i].Name, Bones[b].gameObject.name) == 0)
                        {
                            //the normalization process for the gpu will "change" the values so we need to do the same here to get the same! 
                            Vector3 correctPOs = Bones[b].position;
                            correctPOs *= normalizationFactor;
                            correctPOs += offset3;
                            correctPOs *= 0.5f;
                            correctPOs *= 2.0f;
                            correctPOs -= offset3;
                            correctPOs *= 360.0f;

                            Vector3 correctedRotation = Bones[b].rotation.eulerAngles;
                            correctedRotation *= normalizationFactor;
                            correctedRotation += offset3;
                            correctedRotation *= 0.5f;
                            correctedRotation *= 2.0f;
                            correctedRotation -= offset3;
                            correctedRotation *= 360.0f;

                            //add matrix
                            clipData.AnimationSlots[i].Skinning[animSamplePos] = matrix;
                            clipData.AnimationSlots[i].Pos[animSamplePos] = correctPOs;
                            clipData.AnimationSlots[i].Rot[animSamplePos] = Quaternion.Euler(correctedRotation);
                        }
                    }
                }
                xPos += 4;
                animSamplePos += 1;
                yield return null;  
                totalD += SampleRat;
            }
            AnimationCount++;
        }


        texture.Apply();

        Unity.Collections.NativeArray<float> rawDataNative = texture.GetRawTextureData<float>();
        float[] rawData = rawDataNative.ToArray();
        byte[] byteArray = new byte[rawData.Length * 4];
        Buffer.BlockCopy(rawData, 0, byteArray, 0, byteArray.Length);


        
        TextureAnimationData.AnimData = byteArray;
        TextureAnimationData.TextureSize = new Vector2Int(texture.width, texture.height);
        TextureAnimationData.TextureFormat = texture.format;
        TextureAnimationData.NumberOfBones = Bones.Count;
        TextureAnimationData.SamplingRate = SampleRat;


        string FilePath = "Assets/" + FileNameOut + ".asset";


        //AssetDatabase.CreateFolder("Assets", "CustomFolder");

        AssetDatabase.CreateAsset(TextureAnimationData, FilePath);
        AssetDatabase.SaveAssets();


        //save texture, the encode to png will destroy the precision!
        /* var dirPath = Application.dataPath + "/Animations/";
         if (!Directory.Exists(dirPath))
         {
             Directory.CreateDirectory(dirPath);
         }
         File.WriteAllBytes(dirPath + "IdleAnimation" + ".bytes", byteArray);


         byte[] read = File.ReadAllBytes(dirPath + "IdleAnimation" + ".bytes");
         Debug.Log("read RAWSIZE: " + read.Length);
        */
        Destroy(tmpObj);

         Debug.Log("Done sampling");
    }


    void AddBoneChilds(Transform  Bone, ref List<Transform> InBones)
    {
        InBones.Add(Bone);
        for (int i = 0; i < Bone.childCount; i++)
        {
            AddBoneChilds(Bone.GetChild(i), ref InBones);
        }
    }
}
#endif