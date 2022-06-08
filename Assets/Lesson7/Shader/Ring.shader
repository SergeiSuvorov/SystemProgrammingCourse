// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Ring"
{
    Properties
    {
		_MainColor("Main Color", Color)=(1,1,1,1)
        _MainTexture("Main Texture", 2D)="white"{}
        
		_Radius ("Radius", Float) = 1

		_OutLineRadius ("OutLineRadius", Float) = 1
    }
    SubShader
    {
        Tags
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 	
		}
        LOD 100

        Pass
        {
			Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
           // #pragma multi_compile_fog

            #include "UnityCG.cginc"

			sampler2D _MainTexture;
            float4 _MainTexture_ST;
			float _Radius;
			float _OutLineRadius;
			fixed4 _MainColor;

			  struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
				float4 normal: NORMAL;
            };
            struct v2f
            {
                float4 vertex:SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTexture);
				
				//for question
				/*
				float x = o.texcoord.x-0.5;
				float y = o.texcoord.y-0.5;
				half rad = sqrt(x * x + y * y);
			
               if(rad>_Radius && rad<_OutLineRadius)
				{
				o.color.a=1;
				}
				else 
				{
				o.color.a=0;
				}
				*/
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
			    fixed4 col = tex2D(_MainTexture, i.texcoord);

                // working

				float x = i.texcoord.x-0.5;
				float y = i.texcoord.y-0.5;
				half rad = sqrt(x * x + y * y);


				if(rad>_Radius && rad<_OutLineRadius)
				{
				col.a=1;
				}
				else 
				{
				col.a=0;
				}

				//for question
				//col.a = i.color.a;

                return col*_MainColor;
            }
            ENDCG
        }
    }
}
