#ifndef __REDPLANT_TUBELIGHT_HEADER_
#define __REDPLANT_TUBELIGHT_HEADER_
// redPlant Tube Lights

	
	// Tube lighting
	// returns attenuation, diffuse light dir, NdotL, specular attenuation
	void redLightTube(RedLight redLight, half3 pos, half3 normal, half3 eyeVec, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, float range) {
			
		// width, height, packMult
		half4 size = redLight.size;
		half3 posLight = redLight.pos;
		
		// get light coordinate space
		half3 lightNormal = redLight.lightNormal;
		half3 lightUp = redLight.lightUp;
		half3 lightRight = redLight.lightRight;
		
		// perfect reflection
		half3 R = reflect(eyeVec, normal);
					
		half halfWidth = size.x * 0.5 * 2;
		half halfHeight = size.y * 0.5;
		
#ifdef REDLIGHT_FAST
		
		// approx two sphere lights
		half3 L0 = (posLight + lightRight * halfWidth) - pos;
		half3 L1 = (posLight - lightRight * halfWidth) - pos;
		// tube approximation (center)
		half3 Ld = L1 - L0;
		
		// diffuse and specular
	
		//float tNom = dot(R,L0) * dot(R,Ld) - dot(L0, Ld);
		//float tDenom = dot(Ld,Ld) - (dot(R, Ld) * dot(R, Ld));
		//float t = tNom / tDenom;
		//TODO: check if this or the version above is faster (precomputing some values)
		const half RoL0 = dot( R, L0 );
		const half RoLd = dot( R, Ld );
		const half L0oLd = dot( L0, Ld );
		const half sqrDistLd = dot(Ld,Ld);
		const half t = ( RoL0 * RoLd - L0oLd ) / ( sqrDistLd - RoLd * RoLd );
	
		half3 Lunormalized = L0 + saturate(t) * Ld;

		// same as closest point on sphere (size.y = height)
		half tubeRad = size.y * (1.0 / PI);
		half3 centerToRay	= dot( Lunormalized, R ) * R - Lunormalized;
		Lunormalized = Lunormalized + centerToRay * clamp( tubeRad / length( centerToRay ), 0.0, 1.0 );
		
		// diffuse attenuation
		half distLight = length(Lunormalized);
		
		// N dot L
		NdotL = LambertTerm(normal, Lunormalized / distLight);
		
		// diffuse and specular light dir
		lightDir = Lunormalized / distLight;
		
		// tube lights have no horizon atm
		specularAtten = 1.0;
		
		// attenuation
		diffuseAtten = calculateAttenuationTube(distLight, halfHeight, halfWidth, range, redLight.falloffIdx);
#else
			
		half3 Ldiffuse = projectOnCylinder(pos, posLight.xyz, lightRight, halfHeight, halfWidth);
			
		// attenuation
		diffuseAtten = calculateAttenuationTube(length(Ldiffuse - pos), halfHeight, halfWidth, range, redLight.falloffIdx);
				
		// N dot L
		NdotL = LambertTerm(normal, normalize(Ldiffuse - pos));
		
		//FIXME: here we do not need horizon clipping?!
		specularAtten = 1.0;
		
		// approx two sphere lights
		half3 L0 = (posLight + lightRight * halfWidth) - pos;
		half3 L1 = (posLight - lightRight * halfWidth) - pos;
		// tube approximation (center)
		half3 Ld = L1 - L0;
		
		// diffuse and specular
	
		//float tNom = dot(R,L0) * dot(R,Ld) - dot(L0, Ld);
		//float tDenom = dot(Ld,Ld) - (dot(R, Ld) * dot(R, Ld));
		//float t = tNom / tDenom;
		//TODO: check if this or the version above is faster (precomputing some values)
		const half RoL0 = dot( R, L0 );
		const half RoLd = dot( R, Ld );
		const half L0oLd = dot( L0, Ld );
		const half sqrDistLd = dot(Ld,Ld);
		const half t = ( RoL0 * RoLd - L0oLd ) / ( sqrDistLd - RoLd * RoLd );
	
		half3 Lunormalized = L0 + saturate(t) * Ld;

		// same as closest point on sphere (size.y = height)
		half tubeRad = size.y * (1.0 / PI);
		half3 centerToRay	= dot( Lunormalized, R ) * R - Lunormalized;
		Lunormalized = Lunormalized + centerToRay * clamp( tubeRad / length( centerToRay ), 0.0, 1.0 );
				
		// diffuse and specular light dir
		//lightDir = Lunormalized / distLight;
		lightDir = normalize(Lunormalized);
		
		// tube lights have no horizon atm
		specularAtten = 1.0;
		
#endif
		
	}
	
	
	
#endif	
	
