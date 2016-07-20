Shader "redPlant/ScreenPos" 
{
	Properties 
	{
		_Scale ("Scale", Float) = 1
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		Pass 
		{
			Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma exclude_renderers flash
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma target 3.0

			#include "UnityCG.cginc"
			
			struct appdata 
			{
				float4 pos : POSITION;
				fixed4 color : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uvScreen : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;

				float4 pos = mul(UNITY_MATRIX_MVP, v.pos);
				o.pos = pos;
				
				o.uvScreen = ComputeScreenPos(o.pos);
				return o;
			}
			
			half4 frag(v2f i) : COLOR0
			{
				fixed2 uv = i.uvScreen.xy / i.uvScreen.w;
			
				float4 c = float4(0, 0, 0, 0);
				c.rgb = float3(uv.x, uv.y, 0);
				c.a = 1.0;
				
				return c;
			}
			ENDCG
		}
	} 
}