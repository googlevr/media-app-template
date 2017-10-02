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


/// Used to render Media Screens for the Media App Template
/// Supports Mono, LeftRight, and TopBottom stereo modes.
Shader "MediaAppTemplate/PhotoScreen" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
    [KeywordEnum(None, TopBottom, LeftRight)] _StereoMode ("Stereo mode", Float) = 0
  }

  SubShader {
  Tags { "RenderType"="Opaque" }
  LOD 100

  Cull Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile ___ _STEREOMODE_TOPBOTTOM _STEREOMODE_LEFTRIGHT

      #include "UnityCG.cginc"

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float _LeftRight;
      float _TopBottom;

      v2f vert(appdata_base v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);

        #ifdef _STEREOMODE_TOPBOTTOM
          o.uv.y *= 0.5;
          if (unity_StereoEyeIndex == 0) {
            o.uv.y += 0.5;
          }
        #endif  // _STEREOMODE_TOPBOTTOM
        #ifdef _STEREOMODE_LEFTRIGHT
          o.uv.x *= 0.5;
          if (unity_StereoEyeIndex != 0) {
            o.uv.x += 0.5;
          }
        #endif  // _STEREOMODE_LEFTRIGHT

        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        fixed4 col = tex2D(_MainTex, i.uv);
        return col;
      }
      ENDCG
    }
  }
}