// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

 Shader "Magic Leap/BRDF/BRDFMultiMaskWipeTransitionDisplace"
 {
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_Color2 ("Alt Color", Color) = (1,1,1,1)
		_MainTex ("Color Map/Alpha Spec", 2D) = "white" {}
		_MaskTex ("RGB Mask", 2D) = "white" {}
		_RampLight ("RampLight", 2D) = "Black" {}
		_Fade ("Fade", Range(0.0,2.0)) = 1.0
		_Luma ("LumaBoost", Range(0.0,2.0)) = 0.0
		_Luma1 ("LumaBoost", Range(0.0,2.0)) = 0.0
		_Luma2 ("LumaBoost", Range(0.0,2.0)) = 0.0
		_Transition ("Transition", Range(0.0,1.0)) = 0.0
		_Clamp ("Clamp", Range(-1,5)) = -1
		_LightVector ("LightVector", Vector) = (0,0,0,0)
		
		_Noise("Noise (RGB)", 2D) = "gray" {}
		_Scale("World Scale", Range(0,3)) = 0.004
        _AnimSpeed("Animation Speed X", Range(-2,2)) = 1

        _Height("Noise Height", Range(0,2)) = 0.8
        _Strength("Noise Emission Strength", Range(0,2)) = 0.3
    }

	SubShader
	{
		Tags { "RenderType" = "opaque" "Queue"="geometry"}
      
		Lighting Off
		CGPROGRAM
		#pragma surface surf RampLight vertex:displace novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_MaskTex;
			float2 uv_Ramp;
			float3 viewDir;
            float4 noiseProject;
		};

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _RampLight;
		sampler2D _Noise;
		half _Luma;
		half _Luma1;
		half _Luma2;
		half _Fade;
		half _Transition;
		half _Clamp;
		float4 _Color;
		float4 _Color2;
		float4 _LightVector;
		float _Scale;
		float _Strength;
		float _Height;
        float _AnimSpeed;
		
		struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        };
 

		half4 LightingRampLight (SurfaceOutput s, half3 lightDir, half3 viewDir, fixed atten)
		{
			float light = dot(s.Normal,_LightVector);
			float rim = dot(s.Normal,viewDir);
			float diff = (light*.5)+.5;
				
			float2 brdfdiff = float2(rim, diff);
			float3 BRDFLight = tex2D(_RampLight, brdfdiff.xy).rgb;
		
			half3 BRDFColor = (s.Albedo);
			
      
			half4 c;
			c.rgb =BRDFColor*lerp(BRDFLight*_Fade,BRDFLight*(_Fade*1.8),_Transition);
			c.a = _Color.a;
			return c;
		}
         void displace(inout appdata_full v, out Input o)
       {
        UNITY_INITIALIZE_OUTPUT(Input, o);
 
        float3 worldSpaceNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal.xyz));
        float3 worldNormalS = saturate(pow(worldSpaceNormal * 1.4, 4));
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
 
        float Speed = _Time.x * _AnimSpeed; 

       
        float4 xy = float4((worldPos.x * _Scale) - Speed, (worldPos.y * _Scale)- Speed, 0, 0);
        float4 xz = float4((worldPos.x * _Scale) - Speed, (worldPos.z * _Scale) - Speed, 0, 0);
        float4 yz = float4((worldPos.y * _Scale) - Speed, (worldPos.z * _Scale) - Speed, 0, 0); 
 
        float4 noiseXY = tex2Dlod(_Noise, xy);
        float4 noiseXZ = tex2Dlod(_Noise, xz );
        float4 noiseYZ = tex2Dlod(_Noise, yz);
 
        o.noiseProject = noiseXY; 
        o.noiseProject = lerp(o.noiseProject, noiseXZ, worldNormalS.y);
        o.noiseProject = lerp(o.noiseProject, noiseYZ, worldNormalS.x);
 
        v.vertex.xyz += (v.normal *(o.noiseProject * _Height)); 
    }
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 maintex = tex2D (_MainTex, IN.uv_MainTex);
			half4 masktex = tex2D (_MaskTex, IN.uv_MaskTex);
			half3 detail = saturate(((masktex.r-1)+(maintex.r*_Clamp)))*-1+1;
			half3 gradient = lerp(_Color,_Color2,masktex.r);
			half texmask = lerp(maintex.a,1,masktex.a);
			half3 gradientmask = lerp(_Color,1,texmask);
			
			half luma1 = masktex.g*_Luma1;
			half luma2 = masktex.b*_Luma2;
			half3 colorcomp = lerp(gradient,gradientmask*(gradient*.7*gradient),_Transition);
			o.Albedo = lerp(0,colorcomp,detail);
			o.Emission = lerp(0,(luma1+luma2)*texmask,_Transition)*gradient;
		}
		ENDCG
	}

    Fallback "Diffuse"
}