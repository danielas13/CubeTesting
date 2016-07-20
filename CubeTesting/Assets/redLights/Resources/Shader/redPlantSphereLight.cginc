#ifndef __REDPLANT_SPHERELIGHT_HEADER_
#define __REDPLANT_SPHERELIGHT_HEADER_
// redPlant Sphere Lights

	// Spherical lighting
	// returns diffuse attenuation, light dir, NdotL, specular attenuation
	void redLightSphereColor(RedLight redLight, sampler2D lightTexture, half3 pos, half3 normal, half3 eyeVec, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, out half3 lightColor, float range) 
	{
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
		
		// NdotL using point light pos
		NdotL = LambertTerm(normal, L);
		
		diffuseAtten = calculateAttenuationSphere(lightDist, lightRadius, range, redLight.falloffIdx);
		
		specularAtten = 1.0;
		
		// not change (no texture)
		lightColor = half3(1,1,1);
	}
	
#endif	
	
