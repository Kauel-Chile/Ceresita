Shader "Unlit/SetColorAlphaMask"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_TargetColor("Target Color", Color) = (0.058,0.135,0.331,1)
		_LumWall("Luminance", Range (0, 1)) = 0.5
		_LumWallDelta("LuminanceDelta", Range (0, 0.5)) = 0.25
	}

	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float3 HUEtoRGB(in float H) {
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R,G,B));
			}

			float Epsilon = 1e-4;
 
			float3 RGBtoHCV(in float3 RGB)
			{
			// Based on work by Sam Hocevar and Emil Persson
			float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
			float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
			float C = Q.x - min(Q.w, Q.y);
			float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
			return float3(H, C, Q.x);
			}

			float3 HSVtoRGB(in float3 HSV)
			{
			float3 RGB = HUEtoRGB(HSV.x);
			return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}

			float3 HSLtoRGB(in float3 HSL)
			{
			float3 RGB = HUEtoRGB(HSL.x);
			float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
			return (RGB - 0.5) * C + HSL.z;
			}

			
 
			float3 RGBtoHSV(in float3 RGB)
			{
			float3 HCV = RGBtoHCV(RGB);
			float S = HCV.y / (HCV.z + Epsilon);
			return saturate(float3(HCV.x, S, HCV.z));
			}

			float3 RGBtoHSL(in float3 RGB)
			{
			float3 HCV = RGBtoHCV(RGB);
			float L = HCV.z - HCV.y * 0.5;
			float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
			return saturate(float3(HCV.x, S, L));
			}

			float3 rgb2hsv(float3 c) {
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-4;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 hsv2rgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TargetColor;
			float _LumWall;
			float _LumWallDelta;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 colhsl = RGBtoHSL(col.rgb);
				float3 targethsl = RGBtoHSL(_TargetColor.rgb);
				float deltal = colhsl.z - _LumWall;
				colhsl.xy = targethsl.xy;
				colhsl.z = saturate(targethsl.z + deltal);
				colhsl.rgb = HSLtoRGB(colhsl.rgb);
				
				col.rgb = lerp(col.rgb,colhsl.rgb,col.a);
				col.a = 1;
				return col;
			}
			ENDCG
		}
	}
}
