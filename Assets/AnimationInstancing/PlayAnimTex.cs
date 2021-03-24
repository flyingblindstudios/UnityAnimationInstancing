using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayAnimTex : MonoBehaviour
{
    [SerializeField] private TextureAnimationData AnimData = null;
    [SerializeField] public float AnimationSpeed = 1.0f;


    //static Texture2D CurrentAnimation;
    static int AnimPropID;
    float CurrentAnimationTime = 0;
    
    MaterialPropertyBlock props;
    MeshRenderer mRender;

    TextureAnimationData.AnimationClip ClipToPlay = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!AnimData)
        { 
            return;
        }

        AnimPropID = Shader.PropertyToID("_AnimTime");
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_AnimTexture", AnimData.GetTexture());

        mRender = GetComponent<MeshRenderer>();
        CurrentAnimationTime = Random.Range(0f,10f);
        props = new MaterialPropertyBlock();

        ClipToPlay = AnimData.AnimationClips[0];

        if (ClipToPlay == null)
        {
            enabled = false;
            Debug.Log("Animaiton clip empty!");
        }
    }
   

    private void Update()
    {
        props.SetFloat(AnimPropID, (CurrentAnimationTime/ ClipToPlay.Length *( ClipToPlay.EndFrame - ClipToPlay.StartFrame)) + ClipToPlay.StartFrame);

        mRender.SetPropertyBlock(props);

        CurrentAnimationTime += Time.deltaTime*AnimationSpeed;

        if (CurrentAnimationTime > ClipToPlay.Length)
        {
            CurrentAnimationTime = 0;
        }
    }


}
