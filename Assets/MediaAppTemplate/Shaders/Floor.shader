// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "MediaAppTemplate/FloorShader" { 
  Properties {
  }
  SubShader {
    Tags {
      "Queue"="Transparent" "RenderType"="Transparent"
    }

    CGPROGRAM
      #pragma surface surf LightOnly alpha:fade

      half4 LightingLightOnly (SurfaceOutput s, half3 lightDir, half atten) {
        half NdotL = dot (s.Normal, lightDir);
        half4 c;
        c.rgb = _LightColor0.rgb * (NdotL * atten * 2);
        c.a = NdotL;
        return c;
      }

      struct Input {
        float2 uv_MainTex;
      };

      void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = 0;
        o.Alpha = 1;
      }
    ENDCG
  }
  Fallback "Diffuse"
}
