#ifndef __REDPLANT_DISKLIGHT_HEADER_
#define __REDPLANT_DISKLIGHT_HEADER_
// redPlant Disk Lights
	
	sampler2D _DiscLUT;
	
	float _Angle;
	float _AngleFalloff;
	
	// Disk lighting
	// returns diffuse attenuation, light dir, NdotL, specular attenuation
	void redLightDisk(RedLight redLight, half3 pos, half3 normal, half3 eyeVec, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, out half3 lightColor, float range) {
		
		//
		const half3 posLight = redLight.pos;
		const half4 size = redLight.size;
		const half lightRadius = size.x * 0.5;
		const half angle = size.z * 0.5;
		
		// DISK ATTENUATION
		half3 posOnPlane = projectOnPlane(pos, redLight.pos, redLight.lightNormal);
		half3 dir = redLight.pos - posOnPlane;
		half dist = max(0, length(dir) - lightRadius);
		half3 nearest = posOnPlane + normalize(dir) * (dist);
		
		lightDir = normalize(nearest - pos);		
		
		NdotL = max(dot(lightDir, redLight.lightNormal * -1), 0.0);	
		half fallOff = -(pow(max(acos(NdotL)/(angle + 0.01), 0.1), _AngleFalloff)) + 1;
		const half NdotL2 = max(dot(normal, lightDir), 0.0);
	
		diffuseAtten = max(calculateAttenuationDisk(length(pos - nearest), lightRadius, range, redLight.falloffIdx) * fallOff * NdotL2, 0.0);
		
		// SAME as SPHERE
		
		// perfect reflection
		const half3 reflectVec = reflect(eyeVec, normal);
		
		// diffuse
		const half3 Lunormalized = posLight - pos;
		const half sqrDist = dot(Lunormalized, Lunormalized);
		
		// this is needed for a good approx for specular reflection
		// but the specular direction for is not good for sphere hit detection
		const half3 centerToRay = dot(Lunormalized, reflectVec) * reflectVec - Lunormalized;
		const half3 closestPoint = Lunormalized + centerToRay * clamp( lightRadius / length( centerToRay ), 0.0, 1.0);	

		// light direction
		lightDir = normalize(closestPoint);
	
		specularAtten = 1.0;
		
		lightColor = half3(1,1,1);
		
	}
	
	
#endif	
