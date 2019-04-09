// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

  Shader "Magic Leap/Unlit/UnlitCutoff" {
    Properties {
      
 
      _MainTex ("Texture", 2D) = "Black" {}
      _Cutoff ("Multiplier", Range(0.0,1)) = 0.0
      _Flash("Flash", Range(0.0,3)) = 0.0
      _Fade("Fade", Range(0.0,1)) = 1.0
      


      
    }
    SubShader {
      Tags { "Queue"="Transparent" "RenderType" = "Transparent" }
      LOD 200
      Lighting Off
      Cull off
      Blend One One 
      Fog { Mode Off}
      CGPROGRAM
     #pragma surface surf Unlit halfasview novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap

      half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half3 viewDir) {
		half3 h = normalize (lightDir + viewDir);


          half4 c;
          c.rgb = s.Albedo;
          c.a = s.Alpha;
          return c;
      }
      struct Input {
          float2 uv_MainTex;
      
      };
      
      sampler2D _MainTex;
      float _Cutoff;
      float _Flash;
      float _Fade;
   


      
      void surf (Input IN, inout SurfaceOutput o) {
	      half4 maintex = tex2D (_MainTex, IN.uv_MainTex);
	      half cutoff = ceil(maintex.r-1+_Cutoff);
	      half flash = maintex.g*_Flash;
      	  float mask =maintex.a;

          o.Emission = saturate(cutoff+flash)*_Fade;

          
          
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }