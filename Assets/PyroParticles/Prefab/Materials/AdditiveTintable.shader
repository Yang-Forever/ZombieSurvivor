Shader "Custom/AdditiveTintable"
{
    Properties
    {
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
        _TintColor ("Tint Color (RGB)", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "Queue"="Transparent+100"
            "LightMode"="Always"
        }

        LOD 200

        Pass
        {
            Cull Back
            Lighting Off

            ZWrite Off
            ZTest Always

            Blend SrcAlpha One  // Additive
            ColorMask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR0;
                float4 pos      : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _TintColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                return col * i.color;
            }
            ENDCG
        }
    }

    Fallback "Particles/Additive"
}
