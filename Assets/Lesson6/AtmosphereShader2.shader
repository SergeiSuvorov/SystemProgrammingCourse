Shader "Unlit/AtmosphereShader2"
{
    Properties
    {
        _Color("Color", Color)=(1,1,1,1)
        _MainTexture("Main Texture", 2D)="white"{}
        

		_Emission("Emission", Color) = (1,1,1,1)
		_Height("Height", Range(-1,1)) = 0
		_Seed("Seed", Range(0,10000)) = 10
		
		_GroundColor ("GroundColor", Color) = (1,1,1,1)
		_SeaColor ("SeaColor", Color) = (1,1,1,1)
		_MountainColor ("MountainColor", Color) = (1,1,1,1)
    

		_HasAtmosphere("HasAtmosphere", Range(-1,1)) =0
        _AtmosphereColor("Atmosphere Color", Color)=(1,1,1,1)
        _AtmosphereRadius("Atmosphere Radius", Range(0.0,2.5))=1.1
        _Step("Step", Range(-2.5,2.5))=1.1
        _Pow( "Pow", int) =1
        _Degrees ("Degrees", Float) = 1
        _Strenght("Strenght", Float) = 1        

    }
    
    SubShader
    {
	Tags
		{ 
			//"Queue" = "Transparent" 
			"RenderType" = "Transparent" 	
			"Queue" = "Transparent" 
			//"RenderType"="Opaque" 
		}

         LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"


			fixed4 _Color;
			fixed4 _MountainColor;
			fixed4 _SeaColor;
			fixed4 _GroundColor;
			float4 _Emission;
			float _Height;
			float _Seed;
			sampler2D _MainTexture;
            float4 _MainTexture_ST;

            struct appdata
            {
               float2 uv : TEXCOORD0;
			   fixed4 diff : COLOR0;
			   float4 vertex : SV_POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float height : TEXCOORD1;
				float normal : TEXCOORD2;
				fixed4 diff : COLOR0;
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
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTexture);
				o.height = height;
				o.normal =  nl;
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
                fixed4 col = tex2D(_MainTexture, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col*i.diff;
            }
            ENDCG
        }
        Pass
        {

            //ZWrite Off
           
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex:SV_POSITION;
                float2 dotProduct : TEXCOORD1;
            };


 sampler2D _MainTex;
 float4 _MainTex_ST;
 float4 _AtmosphereColor;
 float _AtmosphereRadius;
 float _Step;
 int _Pow;
 float _Degrees;
 float _Strenght;
 float _HasAtmosphere;

 float _PowParam;
			float _AlphaParam;

v2f vert (appdata_full v)
{
 v2f o;
 float4 cameraLocalPos = mul(unity_WorldToObject, float4( _WorldSpaceCameraPos,1.0 ));
 float4 viewDir = v.vertex - cameraLocalPos;
 float dotProduct = dot(v.normal, normalize(viewDir));
 float atmosphereRadius = _AtmosphereRadius;;
 if(_HasAtmosphere<0.01)
 {
	 atmosphereRadius=0;
	 dotProduct =0;
 }
 v.vertex.xyz*=(1+ atmosphereRadius);
 o.vertex = UnityObjectToClipPos(v.vertex);
 o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
 o.dotProduct.x = abs(dotProduct);
 return o;
}
fixed4 frag (v2f i) : SV_Target
{

  fixed alpha = _Strenght*pow(abs(sin(i.dotProduct.x * radians(_Degrees) + _Step )),_Pow);


 fixed4 color = _AtmosphereColor;

 color.w = alpha;
 return color;

}
ENDCG

}

        
    }
}
