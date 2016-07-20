#ifndef __REDPLANT_LIGHT_HEADER_
#define __REDPLANT_LIGHT_HEADER_

#include "redPlantRectLight.cginc"
#include "redPlantSphereLight.cginc"
#include "redPlantTubeLight.cginc"
#include "redPlantDiskLight.cginc"
#include "redPlantIESLight.cginc"

void redLightFunction(sampler2D lightInfo, half3 posWS, half3 normalWS, half3 eyeVec, inout half smoothness, half reflectivity, inout UnityLight light, 
						out half diffuseAtten, out half specularAtten, out half3 secondColor, RedLight redLight, float range) 
{

	half3 lightColor = half3(1, 1, 1);
	secondColor = half3(1, 1, 1);
	
	if(redLight.type == REDLIGHT_TYPE_RECT) 
	{
		redLightRectangularColor(redLight, lightInfo, posWS, normalWS, eyeVec, light.dir, light.ndotl, diffuseAtten, 
										specularAtten, lightColor, range);
												
		// calculate color (unity does this like this)
		#if defined(REDLIGHT_DEFERRED)
			secondColor = (_LightColor.rgb * lightColor) * diffuseAtten;
		#else
			secondColor = (_LightColor0.rgb * lightColor) * diffuseAtten;
		#endif	
	} 
	else if(redLight.type == REDLIGHT_TYPE_RECT_TEXTURED) {

		redLightRectangularTexture(redLight, lightInfo, posWS, normalWS, eyeVec, smoothness, reflectivity, 
									light.dir, light.ndotl, diffuseAtten, specularAtten, secondColor, lightColor, range);
	}
	else if(redLight.type == REDLIGHT_TYPE_SPHERE) 
	{
		 redLightSphereColor(redLight, lightInfo, posWS, normalWS, eyeVec, light.dir, light.ndotl, diffuseAtten, specularAtten, lightColor, range);	
		 
		 // calculate color (unity does this like this)
		#if defined(REDLIGHT_DEFERRED)
			secondColor = (_LightColor.rgb * lightColor) * diffuseAtten;
		#else
			secondColor = (_LightColor0.rgb * lightColor) * diffuseAtten;
		#endif	
	} 
	else if(redLight.type == REDLIGHT_TYPE_TUBE) 
	{
		redLightTube(redLight, posWS, normalWS, eyeVec, light.dir, light.ndotl, diffuseAtten, specularAtten, range);
		
		// calculate color (unity does this like this)
		#if defined(REDLIGHT_DEFERRED)
			secondColor = (_LightColor.rgb * lightColor) * diffuseAtten;
		#else
			secondColor = (_LightColor0.rgb * lightColor) * diffuseAtten;
		#endif	
	} 
	else if(redLight.type == REDLIGHT_TYPE_DISK) 
	{
		 redLightDisk(redLight, posWS, normalWS, eyeVec, light.dir, light.ndotl, diffuseAtten, specularAtten, lightColor, range);	

		// calculate color (unity does this like this)
		#if defined(REDLIGHT_DEFERRED)
			secondColor = (_LightColor.rgb * lightColor) * diffuseAtten;
		#else
			secondColor = (_LightColor0.rgb * lightColor) * diffuseAtten;
		#endif			 
	} 
	else if(redLight.type == REDLIGHT_TYPE_IES) 
	{
		 redLightIES(redLight, lightInfo, posWS, normalWS, eyeVec, light.dir, light.ndotl, diffuseAtten, specularAtten, lightColor, range);
		
		// calculate color (unity does this like this)
		#if defined(REDLIGHT_DEFERRED)
			secondColor = (_LightColor.rgb * lightColor) * diffuseAtten;
		#else
			secondColor = (_LightColor0.rgb * lightColor) * diffuseAtten;
		#endif	
	} 
	else 
	{
		// error....
		diffuseAtten = 0.0;
		light.dir = half3(0,0,0);
		light.ndotl = 0.0;
		specularAtten = 0.0;
	}


// calculate color (unity does this like this)
#if defined(REDLIGHT_DEFERRED)
	light.color = (_LightColor.rgb * lightColor) * diffuseAtten;
#else
	light.color = (_LightColor0.rgb * lightColor) * diffuseAtten;
#endif	
}
	
#endif	
	
