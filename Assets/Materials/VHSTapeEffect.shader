Shader "Hidden/CleanVHSEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Warmth ("Warmth", Range(0.5, 2)) = 1.1
        _Vignette ("Vignette", Range(0, 2)) = 1
        _ScanLines ("Scan Lines", Range(0, 10)) = 0.1
        _ChromaShift ("Chroma Shift", Range(0, 0.02)) = 0.005
        _TimeOffset ("Time Offset", Float) = 0
        _StaticNoise ("Static Noise", Range(0, 0.1)) = 0.02
        _ScanLineJitter ("Scan Line Jitter", Range(0, 1)) = 0.05

        [Header(Tape Damage)]
        _HeadWobble ("Head Wobble", Range(0, 0.1)) = 0.02
        _HeadWobbleWidth ("Head Wobble Width", Range(1, 10)) = 3
        _TrackingError ("Tracking Error", Range(0, 0.1)) = 0.01
        _TapeNoise ("Tape Noise", Range(0, 0.1)) = 0.05
        
        [Header(Color Effects)]
        _Saturation ("Saturation", Range(0, 2)) = 1.0
        _Contrast ("Contrast", Range(0.5, 1.5)) = 1.0
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
            float _HeadWobble;
            float _HeadWobbleWidth;
            float _TrackingError;
            float _TapeNoise;
            float _Saturation;
            float _Contrast;

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 uv)
            {
                return rand(uv);
            }

            float perlinNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float vignette(float2 uv)
            {
                float2 center = uv - 0.5;
                return 1.0 - dot(center, center) * _Vignette;
            }

            float2 lineShift(float2 uv, float time)
            {
                float2 shift = float2(0, 0);                
                float lineFreq = 300.0 / _HeadWobbleWidth;                
                float linePos = floor(uv.y * lineFreq);

                if (rand(float2(linePos, time * 0.5)) > 0.995)
                {
                    float wobbleStrength = _HeadWobble * (0.5 + rand(float2(linePos, time * 1.3)) * 0.5);
                    shift.x = (rand(float2(linePos, time * 1.3)) - 0.5) * wobbleStrength;
                    
                    for (int i = 1; i <= _HeadWobbleWidth; i++)
                    {
                        float neighborLinePos = linePos + i;
                        if (rand(float2(neighborLinePos, time * 0.7)) > 0.8)
                        {
                            float falloff = 1.0 - (float)i / (_HeadWobbleWidth + 1);
                            shift.x += (rand(float2(neighborLinePos, time * 2.1)) - 0.5) * wobbleStrength * falloff * 0.3;
                        }
                    }
                }
                
                return shift;
            }

            float2 tapeWobble(float2 uv, float time)
            {
                float2 wobble;
                wobble.x = sin(uv.y * 50.0 + time * 3.0) * _TrackingError * 0.5;
                wobble.y = sin(uv.y * 30.0 + time * 2.0) * _TrackingError;
                return wobble;
            }

            float scanLines(float2 uv, float time)
            {
                float jitter = _ScanLineJitter > 0 ? sin(uv.y * 200.0 + time * 10.0) * _ScanLineJitter * 0.01 : 0;
                float scanLine = sin((uv.y + jitter) * 1000.0 + time * 5.0);
                
                scanLine = abs(scanLine);
                scanLine = 1.0 - scanLine * _ScanLines;
                
                return lerp(0.8, 1.2, scanLine);
            }

            float3 subtleChromaShift(float2 uv, float shift, float time)
            {
                if (shift <= 0.001) 
                {
                    return tex2D(_MainTex, uv).rgb;
                }
                
                float animatedShift = shift * (1.0 + sin(time * 1.5) * 0.5);
                float verticalShift = sin(time * 0.7) * 0.0005;
                
                float3 col;
                col.r = tex2D(_MainTex, uv + float2(animatedShift * 0.7, verticalShift)).r;
                col.g = tex2D(_MainTex, uv + float2(0.0, verticalShift * 0.5)).g;
                col.b = tex2D(_MainTex, uv - float2(animatedShift * 0.5, -verticalShift)).b;
                return col;
            }

            float3 addStaticNoise(float2 uv, float time, float intensity)
            {
                if (intensity <= 0.001) return float3(0,0,0);
                
                float2 noiseUV = uv * float2(1920.0, 1080.0) * 0.1 + time;
                float noise1 = perlinNoise(noiseUV);
                float noise2 = rand(uv + time);
                
                float combinedNoise = (noise1 + noise2) * 0.5 * intensity;
                return float3(combinedNoise, combinedNoise, combinedNoise);
            }

            float3 addTapeNoise(float2 uv, float time)
            {
                if (_TapeNoise <= 0.001) return float3(0,0,0);
                
                float tapeNoise = sin(uv.y * 500.0 + time * 20.0) * 0.5 + 0.5;
                tapeNoise *= rand(float2(floor(uv.y * 100.0 + time * 5.0), time));
                return float3(tapeNoise, tapeNoise, tapeNoise) * _TapeNoise;
            }

            float3 applySaturation(float3 color, float saturation)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luminance, luminance, luminance), color, saturation);
            }

            float3 applyContrast(float3 color, float contrast)
            {
                return (color - 0.5) * contrast + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y + _TimeOffset;
                float2 uv = i.uv;
                float2 wobble = _TrackingError > 0.001 ? tapeWobble(uv, time) : float2(0,0);
                float2 lineShiftUV = _HeadWobble > 0.001 ? lineShift(uv, time) : float2(0,0);                
                float2 distortedUV = uv + wobble + lineShiftUV;
                float3 col = subtleChromaShift(distortedUV, _ChromaShift, time);

                col.r *= _Warmth;
                col.g *= _Warmth * 0.95;
                col.b *= _Warmth * 0.9;
                
                col *= scanLines(uv, time);

                col += addStaticNoise(uv, time, _StaticNoise);
                col += addTapeNoise(uv, time);

                col = applyContrast(col, _Contrast);
                col = applySaturation(col, _Saturation);

                col *= vignette(uv);

                float flash = rand(float2(time * 0.3, time * 0.7)) > 0.998 ? 1.3 : 1.0;
                col *= flash;

                col = saturate(col);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}