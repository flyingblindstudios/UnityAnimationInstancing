using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayAnimTex : MonoBehaviour
{
    [SerializeField] private TextureAnimationData AnimData = null;
    [SerializeField] public float AnimationSpeed = 1.0f;

    [SerializeField] Transform[] Slots = null;

    //static Texture2D CurrentAnimation;
    static int AnimPropID;
    float CurrentAnimationTime = 0;
    
    MaterialPropertyBlock props = null;
    MeshRenderer mRender = null;

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

        float time01 = CurrentAnimationTime / ClipToPlay.Length;


        props.SetFloat(AnimPropID, (time01 * ( ClipToPlay.EndFrame - ClipToPlay.StartFrame)) + ClipToPlay.StartFrame);

        mRender.SetPropertyBlock(props);

        CurrentAnimationTime += Time.deltaTime*AnimationSpeed;

        if (CurrentAnimationTime > ClipToPlay.Length)
        {
            CurrentAnimationTime = 0;
        }

        for (int s = 0; s < Slots.Length; s++)
        {
            //animate slots
            for (int i = 0; i < ClipToPlay.AnimationSlots.Count; i++)
            {
                if (ClipToPlay.AnimationSlots[i].Name == Slots[s].name)
                {
                    int size = ClipToPlay.AnimationSlots[i].Pos.Length;
                    int index = Mathf.Clamp(Mathf.FloorToInt(size * time01),0,size);
                    Slots[s].localPosition = ClipToPlay.AnimationSlots[i].Pos[index];
                    Slots[s].localRotation = ClipToPlay.AnimationSlots[i].Rot[index];
                    break;
                }
            }
        }
       
    }


}
