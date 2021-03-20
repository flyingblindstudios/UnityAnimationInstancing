using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnimationToTexture : MonoBehaviour
{
    [SerializeField] GameObject Character;
    [SerializeField] AnimationClip[] Clips;
    [SerializeField] string RootBoneName = "Hips";

    private List<Transform> Bones = new List<Transform>();
    private List<Matrix4x4> InverseBind = new List<Matrix4x4>();
    private void Start()
    {
        StartCoroutine(Convert());
    }

    int GetIndex(int x, int y, int width)
    {
        return y* width + x;
    }

    private IEnumerator Convert()
    {
        GameObject tmpObj = Instantiate(Character);

        //because of rotation i need 360
        float SampleRat = 1f/30f;

        float totalAnimationTime = 0;
        foreach (AnimationClip clip in Clips)
        {
            totalAnimationTime += clip.length;
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

        Debug.Log("TextureSize: " + " sizeX: " + TextureSizeX + " sizeY: " + TextureSizeY);

        //Color[] textureMap = new Color[TextureSizeY* TextureSizeX];

        Texture2D texture = new Texture2D(TextureSizeX, TextureSizeY, TextureFormat.RGBAFloat, false, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float normalizationFactor = 1.0f/ 360.0f;
    

        float totalD = 0;
        //sample animations
        foreach (AnimationClip clip in Clips)
        {
            
            int xPos = 0;
            for (float d = 0; d < clip.length; d += SampleRat)
            {
                clip.SampleAnimation(tmpObj, d);
                for (int b = 0; b < Bones.Count; b++)
                {
                    int yPos = b;

                    //rootbone worldtolocal to remove root movement

                    Matrix4x4 matrix = Bones[b].localToWorldMatrix  *  InverseBind[b];
                    

                    Vector4 r0 = matrix.GetRow(0) * normalizationFactor;
                    Vector4 r1 = matrix.GetRow(1) * normalizationFactor;
                    Vector4 r2 = matrix.GetRow(2) * normalizationFactor;
                    Vector4 r3 = matrix.GetRow(3) * normalizationFactor;
                    Vector4 offset = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                    r0 = (r0 + offset) * 0.5f;
                    r1 = (r1 + offset) * 0.5f;
                    r2 = (r2 + offset) * 0.5f;
                    r3 = (r3 + offset) * 0.5f;

                    texture.SetPixel(xPos, yPos, r0);
                    texture.SetPixel(xPos+1, yPos, r1);
                    texture.SetPixel(xPos+2, yPos, r2);
                    texture.SetPixel(xPos+3, yPos, r3);

                    if (xPos == 0 && yPos == 0)
                    {
                        Debug.Log("r0 " + r0.ToString("F3"));
                        Debug.Log("r1 " + r1.ToString("F3"));
                        Debug.Log("r2 " + r2.ToString("F3"));
                        Debug.Log("r3 " + r3.ToString("F3"));
                    }


                }
                xPos += 4;
                yield return null;  
                totalD += SampleRat;
            }
            break;
        }


        texture.Apply();

        for (int x = 0; x < 4; x++)
        {
            Debug.Log("old x" + x + ": " + texture.GetPixel(x, 0));
        }

        Unity.Collections.NativeArray<float> rawDataNative = texture.GetRawTextureData<float>();
        float[] rawData = rawDataNative.ToArray();

        Debug.Log("float RAWSIZE: " + rawData.Length);

        byte[] byteArray = new byte[rawData.Length * 4];
        Buffer.BlockCopy(rawData, 0, byteArray, 0, byteArray.Length);

        Debug.Log("byte RAWSIZE: " + byteArray.Length);


         //save texture, the encode to png will destroy the precision!
         var dirPath = Application.dataPath + "/Animations/";
         if (!Directory.Exists(dirPath))
         {
             Directory.CreateDirectory(dirPath);
         }
         File.WriteAllBytes(dirPath + "IdleAnimation" + ".bytes", byteArray);


         byte[] read = File.ReadAllBytes(dirPath + "IdleAnimation" + ".bytes");
         Debug.Log("read RAWSIZE: " + read.Length);

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
