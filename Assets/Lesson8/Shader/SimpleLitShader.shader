Shader "Unlit/SimpleLitShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
	   { 
	   "RenderType"="Opaque" 
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
			
			sampler2D _MainTex;
            float4 _MainTex_ST;

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
				fixed4 diff : COLOR0;
            };

			
			 v2f vert (appdata_full v)
            {
			    v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl=max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				o.diff=nl*_LightColor0;
				o.diff.rgb+=ShadeSH9(half4(worldNormal,1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            fixed4 color =  _Color;
	
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
