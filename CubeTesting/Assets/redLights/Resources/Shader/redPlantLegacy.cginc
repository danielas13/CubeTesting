#ifndef __REDPLANT_LEGACYLIGHT_HEADER_
#define __REDPLANT_LEGACYLIGHT_HEADER_
// redPlant Legacy support
	
#include "Assets/redLights/Resources/Shader/redPlantRectLight.cginc"
#include "Assets/redLights/Resources/Shader/redPlantSphereLight.cginc"
#include "Assets/redLights/Resources/Shader/redPlantTubeLight.cginc"
#include "Assets/redLights/Resources/Shader/redPlantDiskLight.cginc"
#include "Assets/redLights/Resources/Shader/redPlantIESLight.cginc"
		
		
	
	//TODO
	float redLightFunctionLegacy(sampler2D lightInfo, half3 posWS, half3 normalWS) {
		return 1.0f;
	}
		
	//TODO
	float redLightFunctionLegacy(sampler2D lightInfo, half3 posWS, half3 normalWS, half3 eyeVec, inout fixed3 lightDir) {

		//TODO: remove conversion from fixed3 to half3 at lightDir
		

		// setup redLight
		RedLight redLight;
		int index = (int)(floor(tex2Dlod(lightInfo, fixed4(0.5, 0.5, 0, 1)).w * 10.0 + REDLIGHT_TEXEL_OFFSET));
		redLight.type = redLightType(_RedLightData, index);
		redLight.size = redLightDim(_RedLightData, index);
		redLight.pos = redLightPos(_RedLightData, index);
		redLight.lightNormal = redLightDir(_RedLightData, index);
		redLight.lightRight = redLightRight(_RedLightData, index);
		redLight.lightUp = cross(redLight.lightRight, redLight.lightNormal);
		redLight.falloffIdx = 0.0f;
				
		half range = redLightRange(_RedLightData, index);
	
		//tmp
		half3 lightColor = half3(1, 1, 1);
		half tmpNdotL;
		half diffuseAtten;
		half specularAtten;
		
		// write light dir
		if(redLight.type == REDLIGHT_TYPE_RECT) 
		{
			redLightRectangularColor(redLight, lightInfo, posWS, normalWS, eyeVec, lightDir, tmpNdotL, diffuseAtten, specularAtten, lightColor, range);	
		} 
		else if(redLight.type == REDLIGHT_TYPE_RECT_TEXTURED) {

#ifdef REDLIGHT_USE_LAMBERT
			float smoothness = 0.0f;
#else
			// shininess from shader needed for this
			float smoothness = 0.05f;
#endif
			float reflectivity = 0.0f;
			float3 specLightColor;

			redLightRectangularTexture(redLight, lightInfo, posWS, normalWS, eyeVec, smoothness, reflectivity, 
									lightDir, tmpNdotL, diffuseAtten, specularAtten, specLightColor, lightColor, range);
															
			//overwrite light color
			//_LightColor0.rgb = lightColor * redLightIntensity(_RedLightData, index);
			//_LightColor0.rgb = (lightColor + specLightColor) * 0.5 * redLightIntensity(_RedLightData, index);
			
	#if !defined(REDLIGHT_USE_LAMBERT)
			_LightColor0.rgb = (lightColor * (1.0 - specularAtten) + specLightColor * specularAtten) * redLightIntensity(_RedLightData, index);
	#else
			_LightColor0.rgb = lightColor * redLightIntensity(_RedLightData, index);
	#endif
		}
		else if(redLight.type == REDLIGHT_TYPE_SPHERE) 
		{
			 redLightSphereColor(redLight, lightInfo, posWS, normalWS, eyeVec, lightDir, tmpNdotL, diffuseAtten, specularAtten, lightColor, range);	
		} 
		else if(redLight.type == REDLIGHT_TYPE_TUBE) 
		{
			redLightTube(redLight, posWS, normalWS, eyeVec, lightDir, tmpNdotL, diffuseAtten, specularAtten, range);
		} 
		else if(redLight.type == REDLIGHT_TYPE_DISK) 
		{
			 redLightDisk(redLight, posWS, normalWS, eyeVec, lightDir, tmpNdotL, diffuseAtten, specularAtten, lightColor, range);		
		} 
		else if(redLight.type == REDLIGHT_TYPE_IES) 
		{
			 redLightIES(redLight, lightInfo, posWS, normalWS, eyeVec, lightDir, tmpNdotL, diffuseAtten, specularAtten, lightColor, range);
		} 
		else 
		{
			// error....
			lightDir = half3(0,0,0);
			_LightColor0.rgb = float3(0,0,0);
			diffuseAtten = 0.0;
			specularAtten = 0.0;
		}
		
		return diffuseAtten;
	}
	
	
#endif	
	
