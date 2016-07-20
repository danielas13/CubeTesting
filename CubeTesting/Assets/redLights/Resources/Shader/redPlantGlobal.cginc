#ifndef _REDPLANT_GLOBAL_HEADER_
#define _REDPLANT_GLOBAL_HEADER_

#if (SHADER_TARGET < 30) && (SHADER_API_D3D9 || SHADER_API_D3D11_9X)
#error "redLights do not support shader model 2.0"
#endif
	
// When including this header
// add one of the following defines above
// REDLIGHT_FORWARD
// REDLIGHT_DEFERRED
// REDLIGHT_CUSTOMPBS (to force custom pbs functions)	
// REDLIGHT_FAST
// REDLIGHT_APPROX_D

#define REDLIGHT_LIGHTINFOSIZE  512 

#define REDLIGHT_TEXEL_OFFSET 0.5
#define REDLIGHT_TEXEL_DIAMOND 0.375

#define REDLIGHT_TYPE_RECT 0
#define REDLIGHT_TYPE_SPHERE 1
#define REDLIGHT_TYPE_TUBE 2
#define REDLIGHT_TYPE_DISK 3
#define REDLIGHT_TYPE_IES 4
#define REDLIGHT_TYPE_LINE 5
// textured versions
#define REDLIGHT_TYPE_RECT_TEXTURED 6

uniform sampler2D _RedLightData; 

#endif
