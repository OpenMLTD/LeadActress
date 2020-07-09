Shader "Unlit/Eyes"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SubTex ("Highlight Texture", 2D) = "white" {}
        _IrisColor ("Iris Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv_main : TEXCOORD0;
                float2 uv_sub : TEXCOORD1;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv_main : TEXCOORD0;
                float2 uv_sub : TEXCOORD1;
                float4 color : COLOR0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SubTex;
            float4 _IrisColor;
            float4 _MainTex_ST;
            float4 _SubTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_main = TRANSFORM_TEX(v.uv_main, _MainTex);
                o.uv_sub = TRANSFORM_TEX(v.uv_sub, _SubTex);
                o.color = _IrisColor;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col_main = tex2D(_MainTex, i.uv_main);
                fixed4 col_sub = tex2D(_SubTex, i.uv_sub);

                float3 rgb = saturate(col_main.rgb + i.color.rgb * (col_sub.a * col_sub.rgb));
                float a = col_main.a;

                float4 col = float4(rgb, a);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    Fallback "Unlit/Texture"
}
