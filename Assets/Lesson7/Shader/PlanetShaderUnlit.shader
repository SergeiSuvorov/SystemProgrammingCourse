Shader "Unlit/PlanetShaderUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Emission("Emission", Color) = (1,1,1,1)
		_Height("Height", Range(-1,1)) = 0
		_Seed("Seed", Range(0,10000)) = 10
		
		_GroundColor ("GroundColor", Color) = (1,1,1,1)
		_SeaColor ("SeaColor", Color) = (1,1,1,1)
		_MountainColor ("MountainColor", Color) = (1,1,1,1)
    }
    SubShader
    {
       Tags
	   { 
	   "RenderType"="Opaque" 
	   "LightMode"="ForwardBase"
	   }
	   LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			fixed4 _Color;
			fixed4 _MountainColor;
			fixed4 _SeaColor;
			fixed4 _GroundColor;
			float4 _Emission;
			float _Height;
			float _Seed;
			sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float height : TEXCOORD1;
				fixed4 diff: COLOR0;
            };


			float hash(float2 st) 
			{
			return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
			}
			
			float noise(float2 p, float size)
			{
			float result = 0;
			p *= size;
			float2 i = floor(p + _Seed);
			float2 f = frac(p + _Seed / 739);
			float2 e = float2(0, 1);
			float z0 = hash((i + e.xx) % size);
			float z1 = hash((i + e.yx) % size);
			float z2 = hash((i + e.xy) % size);
			float z3 = hash((i + e.yy) % size);
			float2 u = smoothstep(0, 1, f);
			result = lerp(z0, z1, u.x) + (z2 - z0) * u.y * (1.0 - u.x) + (z3 - z1) * u.x * u.y;   
			return result;
			}

            v2f vert (appdata_full v)
            {
			    float height = noise(v.texcoord, 5) * 0.75 + noise(v.texcoord, 30) * 0.125 + noise(v.texcoord, 50) * 0.125;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl=max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.height = height;
				o.diff=nl*_LightColor0;
				o.diff.rgb+=ShadeSH9(half4(worldNormal,1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
			fixed4 color =  _Color;
			float height = i.height;
			if (height < 0.45)
			{
			    color=_SeaColor;
			}
			else if (height < 0.75)   
			{
			    color=_GroundColor;
			}
			else
			{
			    color=_MountainColor;   
			}
                // sample the texture
				i.diff+=color;
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col*i.diff;
            }
            ENDCG
        }
    }
}
