using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TextureAnimationData : ScriptableObject
{
    [Serializable]
    public class AnimationClip
    {
        [SerializeField] public string Name;
        [SerializeField] public int StartFrame;
        [SerializeField] public int EndFrame;
        [SerializeField] public int AnimationIndex;
        [SerializeField] public float Length;
        [SerializeField] public List<AnimationSlot> AnimationSlots = new List<AnimationSlot>();
    }

    [Serializable]
    public class AnimationSlot
    {
        public string Name;
        public Matrix4x4[] Skinning;
        public Vector3[] Pos;
        public Quaternion[] Rot;
    }


    [SerializeField] [HideInInspector] public byte[] AnimData;
    [SerializeField] public Vector2Int TextureSize;
    [SerializeField] public int NumberOfBones;
    [SerializeField] public TextureFormat TextureFormat;
    [SerializeField] public float SamplingRate;
    [SerializeField] public float NormalisationFactor;

    [SerializeField] public List<AnimationClip> AnimationClips = new List<AnimationClip>();
   

    //Slots

    private Texture2D AnimTexture = null;
    



    public Texture2D GetTexture()
    {
        if (!AnimTexture)
        {
            //load texture from bytes
            AnimTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat, false, false);
            AnimTexture.filterMode = FilterMode.Point;
            AnimTexture.wrapMode = TextureWrapMode.Clamp;
            AnimTexture.LoadRawTextureData(AnimData);
            AnimTexture.Apply();
        }
        return AnimTexture;
    }
}
