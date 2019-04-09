// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

 Shader "Magic Leap/BRDF/BRDFMultiMaskWipeTransition"
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
    }

	SubShader
	{
		Tags { "RenderType" = "opaque" "Queue"="geometry"}
      
		Lighting Off
		CGPROGRAM
		#pragma surface surf RampLight novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_MaskTex;
			float2 uv_Ramp;
		};

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _RampLight;
		half _Luma;
		half _Luma1;
		half _Luma2;
		half _Fade;
		half _Transition;
		half _Clamp;
		float4 _Color;
		float4 _Color2;
		float4 _LightVector;

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
			//o.Albedo = detail;
			o.Emission = lerp(0,(luma1+luma2)*texmask,_Transition)*gradient;
		}
		ENDCG
	}

    Fallback "Diffuse"
}