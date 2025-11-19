Shader "Hidden/CleanVHSEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Warmth ("Warmth", Range(0.5, 2)) = 1.1
        _Vignette ("Vignette", Range(0, 0.5)) = 0.3
        _ScanLines ("Scan Lines", Range(0, 0.3)) = 0.1
        _ChromaShift ("Chroma Shift", Range(0, 0.02)) = 0.005
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Warmth;
            float _Vignette;
            float _ScanLines;
            float _ChromaShift;

            float vignette(float2 uv)
            {
                float2 center = uv - 0.5;
                return 1.0 - dot(center, center) * _Vignette;
            }

            float scanLines(float2 uv, float time)
            {
                return 1.0 + (sin(uv.y * 600.0 + time * 2.0) * _ScanLines * 0.5);
            }

            float3 subtleChromaShift(float2 uv, float shift)
            {
                float3 col = tex2D(_MainTex, uv).rgb;
                col.r = tex2D(_MainTex, uv + float2(shift * 0.3, 0)).r;
                col.b = tex2D(_MainTex, uv - float2(shift * 0.3, 0)).b;
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                float2 uv = i.uv;

                float3 col = subtleChromaShift(uv, _ChromaShift);

                col.r *= _Warmth;
                col.g *= _Warmth * 0.95;
                col.b *= _Warmth * 0.8;

                col *= scanLines(uv, time);

                col *= vignette(uv);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}