Shader "redPlant/Handles" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" } 
		
		Pass 
		{
			ZWrite Off
			Cull Off 
			ZTest GEqual 
			
			Blend SrcAlpha OneMinusDstAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
				o.uv = float4( v.texcoord.xy, 0, 0 );
				return o;
			}
			
			half4 _Color;
			half4 frag( v2f i ) : SV_Target 
			{
				half4 c = _Color;
				c.a = 0.1;
				return c;
			}
			ENDCG
		}
		Pass 
		{
			ZWrite Off
			Cull Off 
			ZTest LEqual
			
			Blend SrcAlpha OneMinusDstAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
				o.uv = float4( v.texcoord.xy, 0, 0 );
				return o;
			}
			
			half4 _Color;
			half4 frag( v2f i ) : SV_Target 
			{
				half4 c = _Color;
				return c;
			}
			ENDCG
		}
		
	}
}