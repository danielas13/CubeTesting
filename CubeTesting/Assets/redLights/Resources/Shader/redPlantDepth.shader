Shader "redPlant/Depth" {
Properties {}

SubShader {
	Tags { "RenderType"="Opaque" }
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	struct v2f 
	{
		float4 pos : SV_POSITION;
		float depth : TEXCOORD0;
	};
	
	v2f vert( appdata_base v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.depth = COMPUTE_DEPTH_01;
		return o;
	}
	
	fixed4 frag(v2f i) : SV_Target 
	{
		float d = i.depth;
		return float4 (d, d, d, 1);
	}
	ENDCG
	}
}

SubShader {
	Tags { "RenderType"="Transparent" }
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	struct v2f 
	{
		float4 pos : SV_POSITION;
		float depth : TEXCOORD0;
	};
	
	v2f vert( appdata_base v ) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.depth = COMPUTE_DEPTH_01;
		return o;
	}
	
	fixed4 frag(v2f i) : SV_Target 
	{
		float d = i.depth;
		return float4 (d, d, d, 1);
	}
	ENDCG
	}
}

Fallback Off
}
