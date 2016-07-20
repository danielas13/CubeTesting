#ifndef __REDPLANT_IESLIGHT_HEADER_
#define __REDPLANT_IESLIGHT_HEADER_
// redPlant IES Lights
	
	#include "Assets/redLights/Resources/Shader/redPlantTextureConfig.cginc"
	
	float _IESProfileIndex;
	float _IESMult;
	float _IESOffset;
	
	// Spherical lighting
	// returns attenuation, diffuse light dir, specular light dir, specular attenuation
	void redLightIES(RedLight redLight, sampler2D lightTexture, half3 pos, half3 normal, half3 eyeVec, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, out half3 lightColor, float range) 
	{
		// 
		const half3 posLight = redLight.pos;
		const half4 size = redLight.size;
		const half lightRadius = size.x * 0.5;
		
		// erfect reflection
		const half3 reflectVec = reflect(eyeVec, normal);
		
		// diffuse
		const half3 Lunormalized = posLight - pos;
		const half lightDist = length(Lunormalized);
		
		// this is needed for a good approx for specular reflection
		// but the specular direction for is not good for sphere hit detection
		const half3 centerToRay = dot(Lunormalized, reflectVec) * reflectVec - Lunormalized;
		const half3 closestPoint = Lunormalized + centerToRay * clamp( lightRadius / length( centerToRay ), 0.0, 1.0);	
			
		// direction	
		lightDir = normalize(closestPoint); 
		
		// point light diffuse
		half3 L = Lunormalized / lightDist;
		NdotL = LambertTerm(normal, L);
		
		// IES Profile
		int index = (int) floor(size.y * 3.0 + 0.375);
		half norm = 1.0f / (float)_IESNumProfiles;
		half y = norm * index + norm;
		
		half iesAngle = dot(normalize(redLight.lightUp), L) * _IESMult + _IESOffset; 
		half candela = tex2D(_IESLUT, float2(iesAngle, y)).x;  
		
		diffuseAtten = calculateAttenuation(lightDist * 0.25, lightRadius * 2, range, redLight.falloffIdx) * candela;
		
		// specular direction
		lightDir = normalize(closestPoint);
		specularAtten = 1.0;
		
		// not change (no texture)
		lightColor = half3(1, 1, 1);
	}
	
#endif	
	
