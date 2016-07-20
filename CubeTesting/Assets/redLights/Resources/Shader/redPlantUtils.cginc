#ifndef __REDPLANT_UTILS_HEADER_
#define __REDPLANT_UTILS_HEADER_

#include "Assets/redLights/Resources/Shader/redConfig.cginc"
#include "Assets/redLights/Resources/Shader/redPlantTextureConfig.cginc"

	#define PI 3.14159265359

    float3 linePlaneIntersect(half3 pos, half3 reflected, half3 planeCenter, fixed3 planeNorm) 
	{
      return pos + reflected * (dot(planeNorm, planeCenter - pos) / dot(planeNorm, reflected));
    }
	
	float3 projectOnPlane(half3 pos, half3 planeCenter, fixed3 planeNorm) 
	{
      return pos - dot(pos - planeCenter, planeNorm) * planeNorm;
    }

    float3 projectOnSphere(float3 pos, float3 sphereCenter, float radius) 
	{
      return sphereCenter + (pos - sphereCenter) * (radius / (distance(pos, sphereCenter)));
    }

    float3 projectOnCylinder(float3 pos, float3 center, float3 lightUp, float radius, float height) 
	{
      float3 X = pos - center;
      float d = dot(lightUp, X);
      d = clamp(d, -height, height);
      float3 res = lightUp * d;
      float3 dir = normalize(X - res);
      return res + dir * radius + center;
    }
	
	// range like in editor 0 - x
	half calculateAttenuationUnityPointLight(half dist, half range, float falloffIdx) 
	{
		half rnorm = saturate(dist/(range));
		int index = (int) floor(falloffIdx * 3.0 + 0.375);
		half norm = 1.0f / (float)_CFONum;
		half y = norm * index + norm;
		
		return tex2D(_CFOLUT, float2(rnorm, y)).x;
		
		// return tex2D(_Falloff, float2(rnorm, 0.0)).x;
		//return 1.0 / (1.0 + 25.0 * rnorm * rnorm);
	}
	
	// Range between 0-1
	half calculateAttenuationUnityPointLight01(half range) 
	{
		return 1.0 / (1.0 + 25.0 * range * range);
	}
		
	const half _GlobalAtten_Constant = 0.0f;
	const half _GlobalAtten_Linear = 0.0f;
	const half _GlobalAtten_Quadratic = 1.0f;

	half calculateAttenuationCLQ(half dist, half size, half range)
	{
		return(1 / (_GlobalAtten_Constant + 
				  _GlobalAtten_Linear * dist + 
				  _GlobalAtten_Quadratic * dist * dist));
	}
	
	half calculateAttenuation(half dist, half size, half range, float falloffIdx) 
	{
		return calculateAttenuationUnityPointLight(dist, range, falloffIdx) * size;
		// return calculateAttenuationCLQ(dist, size, range);
	}
	
	//redLight attenuation adjustment
	#define redLight_Rect_Power 		PI
	#define redLight_Sphere_Power 		PI * 0.5
	#define redLight_Tube_Power 		PI * 0.5
	#define redLight_Disk_Power 		PI * 2.0
	
	half calculateAttenuationRect(half dist, half width, half height, half range, float falloffIdx) 
	{
		return calculateAttenuationUnityPointLight(dist, range, falloffIdx) * redLight_Rect_Power;
		//half size = width * height;	
		//return calculateAttenuation(dist, size, range);
	}

	half calculateAttenuationSphere(half dist, half radius, half range, float falloffIdx) 
	{
		return calculateAttenuationUnityPointLight(dist, range, falloffIdx) * radius * redLight_Sphere_Power;
		//half size = 2 * PI * radius * 1.0;	
		//return calculateAttenuation(dist, size, range);
	}

	half calculateAttenuationTube(half dist, half radius, half height, half range, float falloffIdx) 
	{
		return calculateAttenuationUnityPointLight(dist, range, falloffIdx) * redLight_Tube_Power;
		//half size = 2 * PI * radius * 1.0;
		//return calculateAttenuation(dist, size, range);
	}
	
	half calculateAttenuationDisk(half dist, half radius, half range, float falloffIdx) 
	{
		return calculateAttenuationUnityPointLight(dist, range, falloffIdx) * redLight_Disk_Power;
		//half size = 2 * PI * radius;
		//return calculateAttenuation(dist, size, range);
	}
	
	float3x3 RotationFromDirection(float3 direction) 
	{
		float3 column1 = float3(0, 0, 0);
		float3 column2 = float3(0, 0, 0);
		float3 column3 = float3(0, 0, 0);
	
		float3 up = float3(0, 1, 0);
		
		float3 xaxis = normalize(cross(up, direction));
		float3 yaxis = normalize(cross(direction, xaxis));
        
		column1.x = xaxis.x;
        column1.y = yaxis.x;
        column1.z = direction.x;

        column2.x = xaxis.y;
        column2.y = yaxis.y;
        column2.z = direction.y;

        column3.x = xaxis.z;
        column3.y = yaxis.z;
        column3.z = direction.z;		
		
		float3x3 res = float3x3(
		column1, 
		column2, 
		column3);
		
		return res;
	}

	// frostbyte lagarde
	float illuminanceSphereOrDisk( float cosTheta , float sinSigmaSqr ) 
	{
		float sinTheta = sqrt (1.0f - cosTheta * cosTheta );
		float illuminance = 0.0f;
		if ( cosTheta * cosTheta > sinSigmaSqr ) 
		{
			illuminance = PI * sinSigmaSqr * saturate ( cosTheta );
		} 
		else 
		{
			float x = sqrt (1.0f / sinSigmaSqr - 1.0f); // For a disk this simplify to x = d / r
			float y = -x * ( cosTheta / sinTheta );
			float sinThetaSqrtY = sinTheta * sqrt (1.0f - y * y);
			illuminance = ( cosTheta * acos (y) - x * sinThetaSqrtY ) * sinSigmaSqr + atan (sinThetaSqrtY / x);
		}
		return max(illuminance , 0.0f);
	}
	
	inline int redLightIndex(sampler2D lightInfo) 
	{
		return (int)(floor(tex2D(lightInfo, fixed2(0.0, 0.0)).w * 10.0 + REDLIGHT_TEXEL_OFFSET));
	}
	
	float _PackRange;
	inline float4 redLightIndexToUV(int index, float element) 
	{
		float indexToUV = 1.0 / REDLIGHT_LIGHTINFOSIZE;
		float2 uv = float2((index * 8 + element) * indexToUV, 0.5);
		return float4(uv, 0, 1);
	}
		
	inline float redLightType(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 0)).r * 255.0 + REDLIGHT_TEXEL_OFFSET;
	}
		
	inline half redLightIntensity(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 0)).a;
	}		

	inline float3 redLightPos(sampler2D lightData, int index) 
	{
		float4 sample = tex2Dlod(lightData, redLightIndexToUV(index, 1));
		float magn = (sample.w * 2 - 1)  * _PackRange;
		return (sample.xyz * 2 -1)  * magn;
	}
	
	inline half redLightRange(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 0)).b;
	}
	
	inline half redLightCullMultiplier(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 6)).r;
	}

	inline float redLightFalloffIdx(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 5)).a * 255.0;
	}

	inline half3 redLightDir(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 2)) * 2 - 1;
	}
	
	inline half3 redLightRight(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 3)) * 2.0 - 1.0;
	}

	inline half3 redLightUp(sampler2D lightData, int index) 
	{
		return tex2Dlod(lightData, redLightIndexToUV(index, 4)) * 2.0 - 1.0;
	}
	
	inline float4 redLightDim(sampler2D lightData, int index) 
	{
		float4 res = tex2Dlod(lightData, redLightIndexToUV(index, 5));
		
		float range = 1.0 / tex2Dlod(lightData, redLightIndexToUV(index, 0)).y;
		res = res * range - range * 0.5;
		return res;
	}	
	
	struct RedLight
	{
		half4 size;
		half3 pos;
		half3 lightNormal;
		half3 lightUp;
		half3 lightRight;
		int type;
		float falloffIdx;
	};
	
	#define REDLIGHT_EXTRACT_DATA int index = (int)(floor(tex2Dlod(_LightTexture0, fixed4(0.5, 0.5, 0, 1)).w * 10.0 + REDLIGHT_TEXEL_OFFSET)); \
	float type = redLightType(_RedLightData, index);\
	half4 size = redLightDim(_RedLightData, index);\
	half3 pos = redLightPos(_RedLightData, index);\
	half3 lightNormal = redLightDir(_RedLightData, index);\
	half3 up = redLightUp(_RedLightData, index);\
	half3 right = redLightRight(_RedLightData, index);\
	o.redLight_data0.x = type;\
	o.redLight_data0.yzw = size.xyz;\
	o.redLight_data1.xyz = pos.xyz;\
	o.redLight_data2.xyz = right.xyz;\
	o.redLight_data1.w = index;\
	o.redLight_data2.w = redLightRange(_RedLightData, index) * redLightCullMultiplier(_RedLightData, index);

	#define REDLIGHT_FRAGMENT_SETUP	RedLight redLight;\
	redLight.type = (int)(floor(i.redLight_data0.x+REDLIGHT_TEXEL_DIAMOND));\
	redLight.size.xyz = i.redLight_data0.yzw;\
	redLight.size.w = i.redLight_data2.w; \
	redLight.pos = i.redLight_data1.xyz;\
	redLight.lightRight = i.redLight_data2.xyz; \
	redLight.lightNormal = redLightDir(_RedLightData, floor(i.redLight_data1.w+REDLIGHT_TEXEL_OFFSET));\
	redLight.lightUp = cross(redLight.lightRight, redLight.lightNormal); \
	redLight.falloffIdx = redLightFalloffIdx(_RedLightData, floor(i.redLight_data1.w+REDLIGHT_TEXEL_OFFSET)); \
				
	#define RANGE_CULL_MULTIPLIER 1.0	
	
	// do early out
#ifdef REDLIGHT_DO_EARLY_OUT
	//TODO: combine these two
	#define REDLIGHT_EARLY_OUT(posWS, lightPos) \
	if(distance(posWS, lightPos) > (i.redLight_data2.w*RANGE_CULL_MULTIPLIER)) { \
		discard;\
	}
		
	#define REDLIGHT_EARLY_OUT_(posWS) \
	if(distance(posWS, i.redLight_data1.xyz) > (i.redLight_data2.w*RANGE_CULL_MULTIPLIER)) { \
		discard;\
	}	
#else
	//NOOP
	#define REDLIGHT_EARLY_OUT(posWS, lightPos)
	#define REDLIGHT_EARLY_OUT_(posWS)
#endif
	
#endif	
	
