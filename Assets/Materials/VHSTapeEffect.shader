Shader "Hidden/CleanVHSEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Warmth ("Warmth", Range(0.5, 2)) = 1.1
        _Vignette ("Vignette", Range(0, 0.5)) = 0.3
        _ScanLines ("Scan Lines", Range(0, 0.3)) = 0.1
        _ChromaShift ("Chroma Shift", Range(0, 0.02)) = 0.005
        _TimeOffset ("Time Offset", Float) = 0
        _StaticNoise ("Static Noise", Range(0, 0.1)) = 0.02
        _ScanLineJitter ("Scan Line Jitter", Range(0, 0.2)) = 0.05
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
            float _TimeOffset;
            float _StaticNoise;
            float _ScanLineJitter;

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float vignette(float2 uv)
            {
                float2 center = uv - 0.5;
                return 1.0 - dot(center, center) * _Vignette;
            }

            float scanLines(float2 uv, float time)
            {
                float jitter = sin(uv.y * 100.0 + time * 5.0) * _ScanLineJitter;
                float scanLine = sin((uv.y + jitter) * 800.0 + time * 3.0);
                
                return 1.0 + (scanLine * _ScanLines * 0.5);
            }

            float3 subtleChromaShift(float2 uv, float shift, float time)
            {
                float animatedShift = shift * (1.0 + sin(time * 2.0) * 0.3);
                
                float3 col;
                col.r = tex2D(_MainTex, uv + float2(animatedShift * 0.5, sin(time) * 0.001)).r;
                col.g = tex2D(_MainTex, uv).g;
                col.b = tex2D(_MainTex, uv - float2(animatedShift * 0.3, cos(time) * 0.001)).b;
                return col;
            }

            float3 addStaticNoise(float2 uv, float time, float intensity)
            {
                float noise = random(uv + time) * intensity;
                return float3(noise, noise, noise);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y + _TimeOffset;
                float2 uv = i.uv;

                float2 jitteredUV = uv;
                // jitteredUV.x += sin(time * 10.0) * 0.001;
                // jitteredUV.y += cos(time * 8.0) * 0.001;

                float3 col = subtleChromaShift(jitteredUV, _ChromaShift, time);

                col.r *= _Warmth;
                col.g *= _Warmth * 0.95;
                col.b *= _Warmth * 0.8;

                col *= scanLines(uv, time);

                col += addStaticNoise(uv, time, _StaticNoise);

                col *= vignette(uv);

                float flash = random(float2(time, time)) > 0.995 ? 1.2 : 1.0;
                col *= flash;

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}