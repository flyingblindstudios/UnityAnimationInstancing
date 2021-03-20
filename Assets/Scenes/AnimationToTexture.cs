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

        
        //AddBoneChilds(rootBone, ref Bones);
       

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

                    //int xPos = Mathf.FloorToInt((totalD / totalAnimationTime) * (float)TextureSizeX);
                    //. Note that since a bone’s world space transform \mathbf{W}_i maps data from joint local space to world space, the corresponding binding transform is actually the inverse of \mathbf{W}_i
                    ///Matrix4x4 bindingMatrix = Bones[b].localToWorldMatrix.inverse;

                    //(bone's local2world matrix) (bindPoseMatrix) * object-space-pos
                    //Matrix4x4 matrix = Bones[b].localToWorldMatrix * InverseBind[b];
                    
                    //this code should be correct/
                    Matrix4x4 matrix =  Bones[b].localToWorldMatrix  *  InverseBind[b];
                    

                    Vector4 r0 = matrix.GetRow(0) * normalizationFactor;
                    Vector4 r1 = matrix.GetRow(1) * normalizationFactor;
                    Vector4 r2 = matrix.GetRow(2) * normalizationFactor;
                    Vector4 r3 = matrix.GetRow(3) * normalizationFactor;
                    Vector4 offset = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                    r0 = (r0 + offset) * 0.5f;
                    r1 = (r1 + offset) * 0.5f;
                    r2 = (r2 + offset) * 0.5f;
                    r3 = (r3 + offset) * 0.5f;

                    /*Debug.Log(r0);
                    Debug.Log(r1);
                    Debug.Log(r2);
                    Debug.Log(r3);*/
                    // Alpha is suppost to be just not included? because the matrix seems to be 3x4, no i think its the last row that needs to go? so i have only 3xrgba?
                    //remoove row 4 we dont need it

                    //if (r0.magnitude > 1 || r1.magnitude > 1 || r2.magnitude > 1 || r3.magnitude > 1)
                    {
                        //Debug.Log("Error found magnitude bigger 1" + r0);
                        //Debug.Log("Error found magnitude bigger 1" + r1);
                       // Debug.Log("Error found magnitude bigger 1" + r2);
                       // Debug.Log("Error found magnitude bigger 1" + r3);
                    }

                  //  textureMap[GetIndex(xPos, yPos, TextureSizeX)] = r0;
                  //  textureMap[GetIndex(xPos+1, yPos, TextureSizeX)] = r1;
                  //  textureMap[GetIndex(xPos+2, yPos, TextureSizeX)] = r2;
                 //   textureMap[GetIndex(xPos + 3, yPos, TextureSizeX)] = r3;

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




        //create texture
       
            
        //texture.SetPixels(textureMap);
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
         //byte[] bytes = texture.EncodeToPNG();
         var dirPath = Application.dataPath + "/Scenes/";
         if (!Directory.Exists(dirPath))
         {
             Directory.CreateDirectory(dirPath);
         }
         File.WriteAllBytes(dirPath + "IdleAnimation" + ".bytes", byteArray);


         byte[] read = File.ReadAllBytes(dirPath + "IdleAnimation" + ".bytes");
         Debug.Log("read RAWSIZE: " + read.Length);


         //file needs to be clmap, point filter and no power of two and no mipmapping!

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

    /*
    public static Matrix4x4[] CalculateSkinMatrix(Transform[] bonePose,
            Matrix4x4[] bindPose,
            Matrix4x4 rootMatrix1stFrame,
            bool haveRootMotion)
    {
        if (bonePose.Length == 0)
            return null;

        Transform root = bonePose[0];
        while (root.parent != null)
        {
            root = root.parent;
        }
        Matrix4x4 rootMat = root.worldToLocalMatrix;

        Matrix4x4[] matrix = new Matrix4x4[bonePose.Length];
        for (int i = 0; i != bonePose.Length; ++i)
        {
            matrix[i] = rootMat * bonePose[i].localToWorldMatrix * bindPose[i];
        }
        return matrix;
    }*/

}
