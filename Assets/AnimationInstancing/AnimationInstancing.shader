Shader "Custom/AnimationInstancing"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AnimTexture("AnimTexture", 2D) = "white" {}
        [PerRendererData] _AnimTime("AnimTime", Range(0,1000)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert addshadow //fullforwardshadows
        #pragma multi_compile_instancing
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #pragma glsl

        sampler2D _MainTex;
        sampler2D_float _AnimTexture;
        //sampler2D_half
        struct Input
        {
            float2 uv_MainTex;
        };

        struct skinning_appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float3 tangent : TANGENT;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            float4 weights : BLENDWEIGHTS;
            int4 idx : BLENDINDICES;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float4 _AnimTexture_TexelSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(float, _AnimTime)
        UNITY_INSTANCING_BUFFER_END(Props)
        

        void LinearBlendSkinning(uint4 indices, float4 weights, float3 positionIn, float3 normalIn, float3 tangentIn, out float3 positionOut, out float3 normalOut, out float3 tangentOut)
        {
            float offset = 1.0 / (float)_AnimTexture_TexelSize.z;
            float halfOffset = offset * 0.5f;
            float offsetY =  1.0 / (float)_AnimTexture_TexelSize.w;
            float halfOffsetY = offsetY * 0.5f;
            
            positionOut = float3(0,0,0);
            normalOut = float3(0, 0, 0);
            tangentOut = float3(0, 0, 0);

            float blockSize = 4;

            for (int i = 0; i < 4; ++i)
            {
                //texture y is the bone
                //texture x is the matrix
                float frameIndex = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTime);
                float bone = indices[i];

                float posx = offset* blockSize * frameIndex;
                posx += halfOffset;

                float  posy =  (offsetY * bone); 
                posy += halfOffsetY;

                float4 matrixR0 = tex2Dlod(_AnimTexture, float4(posx, posy, 0, 0));
                posx += offset;
                float4 matrixR1 = tex2Dlod(_AnimTexture, float4(posx, posy, 0, 0));
                posx += offset;
                float4 matrixR2 = tex2Dlod(_AnimTexture, float4(posx, posy, 0, 0));
                posx += offset;
                float4 matrixR3 = tex2Dlod(_AnimTexture, float4(posx, posy, 0, 0));
               
                float4 offset = float4(1.0, 1.0, 1.0, 1.0);

                matrixR0 *= 2.0;
                matrixR0 -= offset;
               
                matrixR1 *= 2.0;
                matrixR1 -= offset;
                
                matrixR2 *= 2.0;
                matrixR2 -= offset;
                
                matrixR3 *= 2.0;
                matrixR3 -= offset;
               

                float4x4 skinMatrix;


                skinMatrix[0] = matrixR0 * 360.0;
                skinMatrix[1] = matrixR1 * 360.0;
                skinMatrix[2] = matrixR2 * 360.0;
                skinMatrix[3] = float4(0,0,0,1);

                float3 vtransformed = mul(skinMatrix, float4(positionIn, 1));
                float3 ntransformed = mul(skinMatrix, float4(normalIn, 0));
                float3 ttransformed = mul(skinMatrix, float4(tangentIn, 0));

                positionOut += vtransformed *weights[i];
                normalOut += ntransformed * weights[i];
                tangentOut += ttransformed * weights[i];
            }
        }

        void vert(inout skinning_appdata v) {

//#ifdef UNITY_PASS_SHADOWCASTER
            
            float3 skinnedPos;
            float3 skinnedNormal;
            float3 skinnedTangent;
            
            LinearBlendSkinning(v.idx,v.weights, v.vertex.xyz, v.normal.xyz, v.tangent.xyz, skinnedPos, skinnedNormal, skinnedTangent);

            v.vertex.xyz = skinnedPos;
            v.normal = skinnedNormal;
            v.tangent = skinnedTangent;
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
