using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayAnimTex : MonoBehaviour
{
    [SerializeField] private TextAsset AnimTexture = null;

    Texture2D CurrentAnimation;

    float CurrentAnimationTime = 0;
    float AnimationSpeed = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (!AnimTexture)
        { 
            return;
        }

        byte[] AnimData = AnimTexture.bytes;
        Debug.Log("Reading bytes: " + AnimData.Length);


        CurrentAnimation = new Texture2D(1376, 41,  TextureFormat.RGBAFloat, false,false);
        CurrentAnimation.filterMode = FilterMode.Point;
        CurrentAnimation.wrapMode = TextureWrapMode.Clamp;
        CurrentAnimation.LoadRawTextureData(AnimData);
        CurrentAnimation.Apply();

        GetComponent<MeshRenderer>().material.SetTexture("_AnimTexture", CurrentAnimation);
    }
   

    private void Update()
    {
        GetComponent<MeshRenderer>().material.SetFloat("_AnimTime", CurrentAnimationTime/11.0f*300);

        CurrentAnimationTime += Time.deltaTime*AnimationSpeed;

        if (CurrentAnimationTime > 11.0f)
        {
            CurrentAnimationTime = 0;
        }
    }


}
