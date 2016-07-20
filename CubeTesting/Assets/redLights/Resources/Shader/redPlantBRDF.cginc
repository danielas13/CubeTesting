#ifndef _REDPLANT_BRDF_HEADER_
#define _REDPLANT_BRDF_HEADER_

#include "Assets/redLights/Resources/Shader/redConfig.cginc"

// Custom redPlant PBS Functions
#ifdef REDPLANT_CUSTOMPBS

// debug spec & diffuse multiplier
float _SpecLight_Multiplier;
float _DiffuseLight_Multiplier;

// redPlant Custom PBS Functions
// Use these Functions to override the original functions
// Adjust the Wrapper defines in UnityStandardBRDF.cginc if you change parameters on these.
// This is due to the CG Language who do not support variadic macros
// But you can also use optional parameters

half4 BRDF1_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor)
{
	half roughness = 1-oneMinusRoughness;
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lv = DotClamped (light.dir, viewDir);
	half lh = DotClamped (light.dir, halfDir);

#if UNITY_BRDF_GGX
	half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm (nh, RoughnessToSpecPower (roughness));
#endif

	half nlPow5 = Pow5 (1-nl);
	half nvPow5 = Pow5 (1-nv);
	half Fd90 = 0.5 + 2 * lh * lh * roughness;
	half disneyDiffuse = (1 + (Fd90-1) * nlPow5) * (1 + (Fd90-1) * nvPow5);
	
	//redPlant change
	//!! dirty hack (does something like tonemapping)
#ifdef REDLIGHT_APPROX_D
	D = D / (D+1);
#endif
	
	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now
	half specularTerm = (V * D) * (UNITY_PI/4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)
	if (IsGammaSpace())
		specularTerm = sqrt(max(1e-4h, specularTerm));
	specularTerm = max(0, specularTerm * nl);

	//redPlant change (will be needed for horizon)
	//FIXME: do we miss some pi here??? the results than are more in unity range...
	specularTerm *= specAtten;
	
	half diffuseTerm = disneyDiffuse * nl;

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	diffColor * (gi.diffuse + light.color * diffuseTerm * _DiffuseLight_Multiplier)
                    + specularTerm * lightSpecColor * _SpecLight_Multiplier * FresnelTerm (specColor, lh)
					+ gi.specular * FresnelLerp (specColor, grazingTerm, nv);

	return half4(color, 1);
}


half4 BRDF2_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor)
{
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);

	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);
	half lh = DotClamped (light.dir, halfDir);

	half roughness = 1-oneMinusRoughness;
	half specularPower = RoughnessToSpecPower (roughness);
	// Modified with approximate Visibility function that takes roughness into account
	// Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness 
	// and produced extremely bright specular at grazing angles

	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is cancelled with Pi in denominator

	half invV = lh * lh * oneMinusRoughness + roughness * roughness; // approx ModifiedKelemenVisibilityTerm(lh, 1-oneMinusRoughness);
	half invF = lh;
	// pow(a,b)=exp2(log2(a)*b)
	half specular = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);
	if (IsGammaSpace())
		specular = sqrt(max(1e-4h, specular)); // @TODO: might still need saturate(nl*specular) on Adreno/Mali

	// Prevent FP16 overflow on mobiles
#if SHADER_API_GLES || SHADER_API_GLES3
	specular = clamp(specular, 0.0, 100.0);
#endif
	
	//redPlant change (will be needed for horizon)
	//FIXME: do we miss some pi here??? the results than are more in unity range...
	//specularTerm *= specAtten;
	
	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));
    half3 color =	(diffColor * _DiffuseLight_Multiplier + specular * specColor * _SpecLight_Multiplier) * light.color * nl
    				+ gi.diffuse * diffColor
					+ gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);

	return half4(color, 1);	
}

//sampler2D unity_NHxRoughness;
half3 BRDF3_Direct_RedPlant(half3 diffColor, half3 specColor, half rlPow4, half oneMinusRoughness)
{
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
	// Lookup texture to save instructions
	half specular = tex2D(unity_NHxRoughness, half2(rlPow4, 1-oneMinusRoughness)).UNITY_ATTEN_CHANNEL * LUT_RANGE;
	return diffColor * _DiffuseLight_Multiplier + specular * specColor * _SpecLight_Multiplier;
}

half3 BRDF3_Indirect_RedPlant(half3 diffColor, half3 specColor, UnityIndirect indirect, half grazingTerm, half fresnelTerm)
{
	half3 c = indirect.diffuse * diffColor;
	c += indirect.specular * lerp (specColor, grazingTerm, fresnelTerm);
	return c;
}

// Old school, not microfacet based Modified Normalized Blinn-Phong BRDF
// Implementation uses Lookup texture for performance
//
// * Normalized BlinnPhong in RDF form
// * Implicit Visibility term
// * No Fresnel term
//
// TODO: specular is too weak in Linear rendering mode
//sampler2D unity_NHxRoughness;
half4 BRDF3_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor)
{
	half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp

	half3 reflDir = reflect (viewDir, normal);
	half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);
	
	half nl = light.ndotl;
	half nh = BlinnTerm (normal, halfDir);
	half nv = DotClamped (normal, viewDir);

	// Vectorize Pow4 to save instructions
	half2 rlPow4AndFresnelTerm = Pow4 (half2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
	half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
	half fresnelTerm = rlPow4AndFresnelTerm.y;

	half grazingTerm = saturate(oneMinusRoughness + (1-oneMinusReflectivity));

	half3 color = BRDF3_Direct_RedPlant(diffColor, specColor, rlPow4, oneMinusRoughness);
	color *= light.color * nl;
	
	color += BRDF3_Indirect_RedPlant(diffColor, specColor, gi, grazingTerm, fresnelTerm);
					
	return half4(color, 1);
	//return half4(1,1,1,1);
}

#else

// proxy functions -> hopefully this gets optimized by compiler
// this could be fixed with preprocessor macros -> android compiler does not support 2 directives for functions
inline half4 BRDF1_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor) {
	return BRDF1_Unity_PBS(diffColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, light, gi);
}

// proxy functions -> hopefully this gets optimized by compiler
// this could be fixed with preprocessor macros -> android compiler does not support 2 directives for functions
inline half4 BRDF2_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor) {
	return BRDF2_Unity_PBS(diffColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, light, gi);
}

// proxy functions -> hopefully this gets optimized by compiler
// this could be fixed with preprocessor macros -> android compiler does not support 2 directives for functions
inline half4 BRDF3_Unity_PBS_RedPlant(half3 diffColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi/*, half alpha*/, half specAtten, half3 lightSpecColor) {
	return BRDF3_Unity_PBS(diffColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, light, gi);
}


#endif

#endif
