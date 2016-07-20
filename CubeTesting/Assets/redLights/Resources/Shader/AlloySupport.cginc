// alloy support header
#ifndef _REDPLANT_ALLOYSUPPORT_HEADER_
#define _REDPLANT_ALLOYSUPPORT_HEADER_

#if defined (DIRECTIONAL_COOKIE)
#include "Assets/redLights/Resources/Shader/redPlantGlobal.cginc"
#include "Assets/redLights/Resources/Shader/redConfig.cginc"

#if defined(UNITY_PASS_DEFERRED)

// DEFERRED RENDERING	
#define REDLIGHT_DEFERRED
	
#elif defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

//FORWARD RENDERING
#define REDLIGHT_FORWARD
#include "Assets/redLights/Resources/Shader/AutoLight.cginc"

#endif // defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD)

#include "Assets/redLights/Resources/Shader/redPlantUtils.cginc"
#include "Assets/redLights/Resources/Shader/redPlantLegacy.cginc"

#endif // defined (DIRECTIONAL_COOKIE)
	
#endif // _REDPLANT_ALLOYSUPPORT_HEADER_
