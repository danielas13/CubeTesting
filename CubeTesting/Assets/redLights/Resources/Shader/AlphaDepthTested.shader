// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Unlit/Color-Alpha" 
{
 Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _Alpha ("Alpha", Range (0, 1)) = 1
    }

SubShader {
      Tags { "Queue" = "Transparent" } 
   LOD 100
         // draw after all opaque geometry has been drawn
		 
		Pass
		{
			  Blend One One
			  ColorMask 0
		}
		 
      Pass {
         ZWrite Off // don't write to depth buffer 
         // in order not to occlude other objects
 
         Blend SrcAlpha OneMinusSrcAlpha // use alpha blending
 
         CGPROGRAM 
 
         #pragma vertex vert 
         #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
 
  struct appdata_t {
    float4 vertex : POSITION;
   };

  struct v2f {
   float4 vertex : SV_POSITION;
   UNITY_FOG_COORDS(o)
  };

  v2f vert (appdata_t v)
   {
    v2f o;
    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
   }


  fixed _Alpha;
  fixed4 _Color;
 
  fixed4 frag (v2f i) : COLOR
  {
   fixed4 col = float4(_Color.r,_Color.g,_Color.b, _Alpha);
   UNITY_APPLY_FOG(i.fogCoord, col);
   //UNITY_OPAQUE_ALPHA(col.a);
   return col;
  }
 
         ENDCG  
      }
   }
}