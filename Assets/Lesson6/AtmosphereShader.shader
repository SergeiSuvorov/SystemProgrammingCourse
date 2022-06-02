Shader "Unlit/AtmosphereShader"
{
    Properties
    {
        _MainColor("Main Color", Color)=(1,1,1,1)
        _MainTexture("Main Texture", 2D)="white"{}
        _OutlineColor("Outline Color", Color)=(1,1,1,1)
        _OutlineSize("OutlineSize", Range(1.0,1.5))=1.1

		_PowParam ("PowParam", Float) = 1
		_AlphaParam ("AlphaParam", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
	Tags
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 	
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _MainColor;
            sampler2D _MainTexture;
            float4 _MainTexture_ST;
            struct appdata
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };
            struct v2f
            {
                float4 clipPos:SV_POSITION;
                float2 uv:TEXCOORD0;
            };
            v2f vert (appdata v)
            {
                v2f o;
                o.clipPos=UnityObjectToClipPos(v.vertex);
                o.uv=(v.uv*_MainTexture_ST.xy)+_MainTexture_ST.zw;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col=tex2D(_MainTexture, i.uv)*_MainColor;
                return col;
            }
            ENDCG
        }
        Pass
        {
            Cull Off

            Fog { Mode Off }
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			fixed4 _Color;
			fixed4 _OutlineColor;
            float _OutlineSize;
			float _Hieght;
			float _PowParam;
			float _AlphaParam;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
				float4 normal: NORMAL;
            };
            struct v2f
            {
                float4 clipPos:SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
                UNITY_FOG_COORDS(1)

            };

            v2f vert (appdata v)
            {
                v2f o;

				float4 vertex =v.vertex;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 distantion = worldPos - _WorldSpaceCameraPos;
				float3 distNormal = normalize(distantion.xyz);
				float3 worldNormal = UnityObjectToWorldNormal(v.normal.xyz);
				float param = dot(distNormal,worldNormal);

                o.clipPos=UnityObjectToClipPos(vertex*_OutlineSize);
				o.color.a = param ;
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                if (IN.color.a >_AlphaParam)
				{
				  half4 tex = tex2D(_MainTex, IN.texcoord);
                    tex.a = 1;
					tex.a = pow(IN.color.a, _PowParam);
                    return tex*_OutlineColor;
				}
                else
                {
                    half4 tex = tex2D(_MainTex, IN.texcoord);
                    tex.a = 0;
                    return tex*_OutlineColor;
                };
            }
            ENDCG
        }
    }
}
