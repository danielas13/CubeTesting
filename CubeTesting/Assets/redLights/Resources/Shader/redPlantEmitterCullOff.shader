Shader "redPlant/EmitterCullOff" 
{
	Properties 
	{
		_Tex ("Main Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_Alpha ("Alpha", Range (0, 1)) = 1
	}

	SubShader {
		Tags { "Queue" = "Transparent" } 
		
		Pass 
		{
		ZWrite Off
		Cull Off 

		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM 

		#pragma vertex vert 
		#pragma fragment frag
		#pragma multi_compile_fog
		#include "UnityCG.cginc"

		struct appdata_t 
		{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
		};

		struct v2f 
		{
			float4 vertex : SV_POSITION;
			float4 texcoord : TEXCOORD0;
		};

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.texcoord = v.texcoord;
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		fixed _Alpha;
		fixed4 _Color;
		sampler2D _Tex;

		fixed4 frag (v2f i) : COLOR
		{
			fixed4 col = float4(_Color.r,_Color.g,_Color.b, _Alpha) * float4(tex2D(_Tex, i.texcoord.xy).rgb, 1.0);
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}

		ENDCG  
		}
	}
}
