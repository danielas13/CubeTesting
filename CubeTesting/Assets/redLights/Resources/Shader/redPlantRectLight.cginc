#ifndef __REDPLANT_RECTLIGHT_HEADER_
#define __REDPLANT_RECTLIGHT_HEADER_
// redPlant Rectangular Lights
		
	// LOD offset fix
#if defined(REDLIGHT_DEFERRED)
	#define LOD_FIX 0.0
#else
	#define LOD_FIX 0.0
#endif
		
	// mrp point from frostbite paper (appendix)
	float3 mrpPoint(const half3 worldPos, const half3 worldNormal, const half3 lightPos, const half3 lightPlaneNormal, 
		const half lightWidth, const half lightHeight, const half3 lightLeft, const half3 lightUp) {
				
		const half clampCosAngle = 0.001 + saturate(dot(worldNormal, lightPlaneNormal));
		// clamp d0 to the positive hemisphere of surface normal
		const half3 d0 = normalize ( -lightPlaneNormal + worldNormal * clampCosAngle);
		// clamp d1 to the negative hemisphere of light plane normal
		const half3 d1 = normalize(worldNormal - lightPlaneNormal * clampCosAngle);
		const half3 dh = normalize(d0 + d1);
		half3 ph = linePlaneIntersect(worldPos, dh, lightPos, lightPlaneNormal);
			
		// project on plane
		const half3 dir = ph - lightPos;
		const half2 diagonal = half2(dot(dir, lightLeft), dot(dir, lightUp));
		const half2 nearest2D = half2(clamp(diagonal.x, -lightWidth, lightWidth), clamp(diagonal.y, -lightHeight, lightHeight));
		ph = lightPos.xyz + (lightLeft * nearest2D.x + lightUp * nearest2D.y);

		return ph;
	}
	
	
	float3 mrpPlane(const half3 worldPos, const half3 worldNormal, const half3 lightPos, const half3 lightPlaneNormal) {
				
		const half clampCosAngle = 0.001 + saturate(dot(worldNormal, lightPlaneNormal));
		// clamp d0 to the positive hemisphere of surface normal
		const half3 d0 = normalize (-lightPlaneNormal + worldNormal * clampCosAngle);
		// clamp d1 to the negative hemisphere of light plane normal
		const half3 d1 = normalize(worldNormal - lightPlaneNormal * clampCosAngle);
		const half3 dh = normalize(d0 + d1);
		half3 ph = linePlaneIntersect(worldPos, dh, lightPos, lightPlaneNormal);
		return ph;
	}
	
	
	// Rectangular lighting
	// returns attenuation, diffuse light dir, specular attenuation
	void redLightRectangularColor(RedLight redLight, sampler2D lightTexture, half3 pos, half3 normal, half3 eyeVec, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, out half3 lightColor, float range) {
	
		// width, height, packMult
		const half4 size = redLight.size;
		const half3 posLight = redLight.pos;
		
		// get light coordinate space
		const half3 lightNormal = redLight.lightNormal;
		const half3 lightUp = redLight.lightUp;
		const half3 lightRight = redLight.lightRight;
				
#if defined(REDLIGHT_FAST)
		// nearest point (closest point on rect)
		const half3 dir = pos - posLight;
		const half2 diagonal = half2(dot(dir, lightRight), dot(dir, lightUp));
		const half2 nearest2D = half2(clamp(diagonal.x, -size.x, size.x), clamp(diagonal.y, -size.y, size.y));
		const half3 nearestPointInside = posLight.xyz + (lightRight * nearest2D.x + lightUp * nearest2D.y);
		half dist = distance(pos.xyz, nearestPointInside);
		
		// write diffuse light dir
		lightDir = normalize(nearestPointInside - pos.xyz);
#else
		// most representative point
		const half3 mrp = mrpPoint(pos, normal, posLight, lightNormal, size.x, size.y, -lightRight, lightUp);
		const half3 unormLightVector = (mrp - pos);
		half dist = length(unormLightVector);			
		// write diffuse light dir
		lightDir = normalize(unormLightVector);	
#endif
		
		NdotL = LambertTerm(normal, lightDir);
		
		// attenuation
		const half attenuation = calculateAttenuationRect(dist, size.x, size.y, range, redLight.falloffIdx);

		//
		half LNdotL = max(dot(lightNormal, -lightDir), 0.0);
		const half NdotL2 = max(dot(normal, lightDir), 0.0);
		
		// diffuse attenuation
		diffuseAtten = attenuation * LNdotL * NdotL2;
		lightColor = half3(1,1,1);
		
		//TODO: here is a problem with traditional forward rendering and custom shader (specAngle is invert)
		
		// reflection vector
		half3 R = reflect(-eyeVec, normal);
		half3 E = linePlaneIntersect(pos.xyz, -R, posLight.xyz, lightNormal);
			
		
		half3 dirSpec = E - posLight.xyz;
		half2 dirSpec2D = half2(dot(dirSpec, lightRight), dot(dirSpec, lightUp));

		half2 nearestSpec2D = half2(clamp(dirSpec2D.x, -size.x, size.x), clamp(dirSpec2D.y, -size.y, size.y));
		half3 specPos = posLight.xyz + (lightRight * nearestSpec2D.x + lightUp * nearestSpec2D.y);
		
		//LH:
		//TODO: looking parallel to rect is a problem...
		// this fades specular at parallel view -> check constant and make global
		specularAtten = saturate(dot(R, lightNormal)*4);	
	
		// this could be useful for high quality
		//half lengthA = length(dirSpec2D - nearestSpec2D);
		//half invLength = 1.0f / (1.0f + lengthA * smoothness);
		//specularAtten = saturate(dot(R, lightNormal)*4) * invLength;
		
		// direction (use this when in deferred mode? (specular attenuation is getting faded out))
		//lightDir = normalize(specPos - pos);
				
		// direction (lerp between diffuse/specular direction)
		lightDir = (1-specularAtten) * lightDir + specularAtten * normalize(specPos - pos);
	}
	
	
	
	// Rectangular lighting
	// returns attenuation, diffuse light dir, specular light dir, specular attenuation
	//TODO: works only for local lights at the moment
	void redLightRectangularTexture(RedLight redLight, sampler2D lightTexture, half3 pos, half3 normal, half3 eyeVec, inout half smoothness, 
									half reflectivity, out half3 lightDir, out half NdotL, out half diffuseAtten, out half specularAtten, out half3 specLightColor, out half3 lightColor, float range) {

		// width, height, packMult
		const half4 size = redLight.size;
		const half3 posLight = redLight.pos;
		
		// get light coordinate space
		const half3 lightNormal = redLight.lightNormal;
		const half3 lightUp = redLight.lightUp;
		const half3 lightRight = redLight.lightRight;
				
#if defined(REDLIGHT_FAST)
		// nearest point (closest point on rect)
		const half3 dir = pos - posLight;
		const half2 diagonal = half2(dot(dir, lightRight), dot(dir, lightUp));
		const half2 nearest2D = half2(clamp(diagonal.x, -size.x, size.x), clamp(diagonal.y, -size.y, size.y));
		const half3 nearestPointInside = posLight.xyz + (lightRight * nearest2D.x + lightUp * nearest2D.y);
		half dist = distance(pos.xyz, nearestPointInside);
		
		// write diffuse light dir
		lightDir = normalize(nearestPointInside - pos.xyz);
#else
		// most representative point
		half3 mrp = mrpPlane(pos, normal, posLight, lightNormal);
				
		// project on plane
		const half3 dir = mrp - posLight;
		const half2 diagonal = half2(dot(dir, lightRight), dot(dir, lightUp));
		const half2 nearest2D = half2(clamp(diagonal.x, -size.x, size.x), clamp(diagonal.y, -size.y, size.y));
		mrp = posLight.xyz + (lightRight * nearest2D.x + lightUp * nearest2D.y);
		
		const half3 unormLightVector = (mrp - pos);
		half dist = length(unormLightVector);			
		// write diffuse light dir
		lightDir = normalize(unormLightVector);
#endif
		NdotL = LambertTerm(normal, lightDir);
		
		// attenuation and
		const half attenuation = calculateAttenuationRect(dist, size.x, size.y, range, redLight.falloffIdx);
		
		const half LNdotL = max(dot(lightNormal, -lightDir), 0.0);
		const half NdotL2 = max(dot(normal, lightDir), 0.0);
		
		// diffuse attenuation
		diffuseAtten = attenuation * LNdotL * NdotL2;
		
		// diffuse color
		half2 uvTex = 1.0f - ((diagonal.xy / (dist + 1.0f)) + float2(size.x, size.y)) / float2(size.x * 2.0f, size.y * 2.0f);
		//uvTex.y = 1.0f - uvTex.y;
		half lod = min(6.0f, pow(dist, 0.1f) * 8.0f + 8.0f) + LOD_FIX;
	
		const half4 t1 = tex2Dlod(lightTexture, float4(uvTex, 0, lod));  
		const half4 t2 = tex2Dlod(lightTexture, float4(uvTex, 0, lod + 1));  
		half3 diffuseColor = lerp(t1, t2, 0.5f).xyz;
		
		// reflection vector
		half3 R = reflect(-eyeVec, normal);
		half3 E = linePlaneIntersect(pos.xyz, -R, posLight.xyz, lightNormal);
			
		half3 dirSpec = E - posLight.xyz;
		half2 dirSpec2D = half2(dot(dirSpec, lightRight), dot(dirSpec, lightUp));

		// change the size of projection
		const half ProjectedRange = 1.0f;
		half2 nearestSpec2D = half2(clamp(dirSpec2D.x, -size.x*ProjectedRange, size.x*ProjectedRange), 
									clamp(dirSpec2D.y, -size.y*ProjectedRange, size.y*ProjectedRange));
		half3 specPos = posLight.xyz + (lightRight * nearestSpec2D.x + lightUp * nearestSpec2D.y);
		
		
		// specular light color
		
		// something like max range but a multiplier (prevent to sample clamp values)
		const half TextureSampleRange = 0.95f;
		half2 uv_coords = (nearestSpec2D * TextureSampleRange / half2(size.x*ProjectedRange,size.y*ProjectedRange)) * 0.5f + 0.5f;
		uv_coords.x = 1.0f - uv_coords.x;
		uv_coords.y = 1.0f - uv_coords.y;
		
		
		// get lod level for specular
		half lod_ = log2(dist*dist*(1.0f-smoothness)*10 + ((1.0f-smoothness)*8)) + LOD_FIX;
		const half3 specColor =  tex2Dlod(lightTexture, float4(uv_coords, 0, lod_)).xyz;	
		
		// for textures check that we not reach 1.0
		smoothness = clamp(smoothness, 0.0, 0.997);
		half lengthA = length(dirSpec2D - nearestSpec2D);
		
		//FIXME: this is dirty blending that blends better between spec and diffuse
		specLightColor = specColor * max(0.f, (1.0f - lengthA * smoothness)) + (1.0 - smoothness) * diffuseColor;
		
		// light sampling
		lightColor = diffuseColor;
		
		// DEFERRED/FORWARD STANDARD SHADER
		//TODO: REDLIGHT_USE_LEGACY should always be defined in legacy shader as we need to test all
		// legacy defines here as this can get included before AutoLight.cginc 
		// which always declares REDLIGHT_USE_LEGACY for legacy shader
#if !defined(REDLIGHT_USE_LEGACY) && !defined(REDLIGHT_USE_NORMAL) && !defined(REDLIGHT_USE_LAMBERT)

		// * 4 to make it brighter, attenuation for a non-unlimited specular light
		specularAtten = saturate(dot(R, lightNormal)*4) * attenuation;	

		// direction (use this when in deferred mode? (specular attenuation is getting faded out))
		lightDir = normalize(specPos - pos);
#else 
		//LEGACY SHADER
	#if !defined(REDLIGHT_USE_LAMBERT)
		// to make it brighter, attenuation for a non-unlimited specular light
		specularAtten = saturate(dot(R, lightNormal)*4) * attenuation;
				
		// direction (lerp between diffuse/specular direction)
		lightDir = (1.0 - specularAtten) * lightDir + specularAtten * normalize(specPos - pos);
	#else
		specularAtten = 0.0;
	#endif
#endif

	}

	
#endif	
	
