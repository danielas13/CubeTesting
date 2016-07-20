#ifndef __REDPLANT_DEFERRED_HEADER_
#define __REDPLANT_DEFERRED_HEADER_
// redPlant Area Lights Header

// Deferred Rendering Support
#if defined(REDLIGHT_DEFERRED)

	// redPlant Deferred/PrePass Rendering Vertex Shader
	// Will be called for every Light Proxy Geometry
	//ORIGINAL: UnityDeferredLibrary.cginc -> vert_deferred
	//TODO: if we do not need this any more than use the builtin function (vert_deferred)
	unity_v2f_deferred redLightVertexDeferred(float4 vertex : POSITION, float3 normal : NORMAL) 
	{
		unity_v2f_deferred o;
		o.pos = mul(UNITY_MATRIX_MVP, vertex);
		o.uv = ComputeScreenPos (o.pos);
		o.ray = mul (UNITY_MATRIX_MV, vertex).xyz * float3(-1,-1,1);
		
		// normal contains a ray pointing from the camera to one of near plane's
		// corners in camera space when we are drawing a full screen quad.
		// Otherwise, when rendering 3D shapes, use the ray calculated here.
		o.ray = lerp(o.ray, normal, _LightAsQuad);
		
#ifdef DIRECTIONAL_COOKIE	
		REDLIGHT_EXTRACT_DATA;
#endif
		return o;
	}	
		
	// redPlant Deferred Rendering Pixel Shader
	// This functions gets called for every proxy light geometry
	// Supports original Directional Lights and Area Lights (directional light cookie)
	//ORIGINAL: Internal-DeferredShading.shader -> CalculateLight
	half4 redLightCalculateDeferred(unity_v2f_deferred i) 
	{
		float3 wpos;
		float2 uv;
		float atten, fadeDist;
		UnityLight light;
		UNITY_INITIALIZE_OUTPUT(UnityLight, light);
					
		// 1 for builtin lights
		half specularAttenuation = 1;
		half3 secondColor = half3(1,1,1);
		
	#if defined(DIRECTIONAL_COOKIE)
		
		// UnityDeferredCalculateLightParams
		i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
		uv = i.uv.xy / i.uv.w;
	
		// read depth and reconstruct world position
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
		depth = Linear01Depth (depth);
		float4 vpos = float4(i.ray * depth,1);
		wpos = mul (_CameraToWorld, vpos).xyz;
	
		REDLIGHT_EARLY_OUT(wpos.xyz, _LightPos.xyz);
		
		half4 gbuffer0 = tex2D (_CameraGBufferTexture0, uv);
		half4 gbuffer1 = tex2D (_CameraGBufferTexture1, uv);
		half4 gbuffer2 = tex2D (_CameraGBufferTexture2, uv);

		half3 baseColor = gbuffer0.rgb;
		half3 specColor = gbuffer1.rgb;
		half oneMinusRoughness = gbuffer1.a;
		half3 normalWorld = gbuffer2.rgb * 2 - 1;
		normalWorld = normalize(normalWorld);
		float3 eyeVec = normalize(wpos-_WorldSpaceCameraPos);
		half oneMinusReflectivity = 1 - SpecularStrength(specColor.rgb);
		
		REDLIGHT_FRAGMENT_SETUP;
		
		float range = i.redLight_data2.w;
		
		// proxy function for light types
		redLightFunction(_LightTexture0, wpos.xyz, normalWorld, eyeVec, oneMinusRoughness, 1.0f-oneMinusReflectivity, 
							light, atten, specularAttenuation, secondColor, redLight, range);
		
	#else
		UnityDeferredCalculateLightParams(i, wpos, uv, light.dir, atten, fadeDist);
		
		half4 gbuffer0 = tex2D (_CameraGBufferTexture0, uv);
		half4 gbuffer1 = tex2D (_CameraGBufferTexture1, uv);
		half4 gbuffer2 = tex2D (_CameraGBufferTexture2, uv);

		half3 baseColor = gbuffer0.rgb;
		half3 specColor = gbuffer1.rgb;
		half oneMinusRoughness = gbuffer1.a;
		half3 normalWorld = gbuffer2.rgb * 2 - 1;
		normalWorld = normalize(normalWorld);
		float3 eyeVec = normalize(wpos-_WorldSpaceCameraPos);
		half oneMinusReflectivity = 1 - SpecularStrength(specColor.rgb);
				
		// original term
		light.ndotl = LambertTerm (normalWorld, light.dir);
	
		// calculate color
		light.color = _LightColor.rgb * atten;
		
	#endif
	
		UnityIndirect ind;
		UNITY_INITIALIZE_OUTPUT(UnityIndirect, ind);
		ind.diffuse = 0;
		ind.specular = 0;
			
		// this calls UNITY_BRDF_PBS for builtin lights
		half4 res = REDPLANT_BRDF_PBS (baseColor, specColor, oneMinusReflectivity, oneMinusRoughness, normalWorld, -eyeVec, light, ind, specularAttenuation, secondColor);
		
		return res;
	}
	
#endif
#endif	
//EOF