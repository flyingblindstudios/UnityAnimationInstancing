using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TextureAnimationData : ScriptableObject
{
    [Serializable]
    public class AnimationClip
    {
        public string Name;
        public int StartFrame;
        public int EndFrame;
        public int AnimationIndex;
        public float Length;
    }

    [Serializable]
    public class AnimationSlot
    {
        public string Name;
        //public int StartFrame;
        //public int EndFrame;
        //public int BlockIndex; //
    }


    [SerializeField] [HideInInspector] public byte[] AnimData;
    [SerializeField] public Vector2Int TextureSize;
    [SerializeField] public int NumberOfBones;
    [SerializeField] public TextureFormat TextureFormat;
    [SerializeField] public float SamplingRate;
    [SerializeField] public float NormalisationFactor;

    [SerializeField] public List<AnimationClip> AnimationClips = new List<AnimationClip>();
    [SerializeField] public List<AnimationSlot> AnimationSlots = new List<AnimationSlot>();

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
