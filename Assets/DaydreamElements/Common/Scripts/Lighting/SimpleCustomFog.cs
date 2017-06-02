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

namespace DaydreamElements.Common {

  using UnityEngine;
  using System.Collections;

  [ExecuteInEditMode]
  public class SimpleCustomFog : MonoBehaviour {

    //[SerializeField] Color primarySpecColor;
    // Sun properties
    //[SerializeField] public Color primaryLightColor;
    //[SerializeField] public float primaryLightIntensity;

    //[SerializeField] public AmbientLight ambientLight;
    
    // Sun position
    //[SerializeField] public float yaw;
    //[SerializeField] public float pitch;

    [SerializeField] public Fog fog;
   
    //[SerializeField] public float sunIntensity;
    //[SerializeField] public float sunBackgroundIntensity;
    //[SerializeField] public float sunBackgroundRadius;
    //[SerializeField] public float sunRadius;
    //[SerializeField] public float skyBackgroundIntensity;

    // Update is called once per frame
    void LateUpdate () {
      /*
      Shader.SetGlobalVector("_PrimaryLightDirection",-transform.forward);
      Shader.SetGlobalVector("_PrimaryLightColor_Intensity", new Vector4(primaryLightColor.r, primaryLightColor.g, primaryLightColor.b, primaryLightIntensity));
      Shader.SetGlobalVector("_AmbientGroundColor_Intensity", new Vector4(ambientLight.ambientGroundColor.r, ambientLight.ambientGroundColor.g, ambientLight.ambientGroundColor.b, ambientLight.ambientGroundIntensity));
      Shader.SetGlobalVector("_AmbientSkyColor_Intensity", new Vector4(ambientLight.ambientSkyColor.r, ambientLight.ambientSkyColor.g, ambientLight.ambientSkyColor.b, ambientLight.ambientSkyIntensity));
      */

      Shader.SetGlobalVector("_FogDistance", new Vector4(1f/(fog.end - fog.start),fog.start,0,0));
      Shader.SetGlobalVector("_FogColorZenith", fog.zenithColor);
      Shader.SetGlobalVector("_FogColorHorizon", fog.horizonColor);
      Shader.SetGlobalVector("_FogColorHorizonDistance", fog.horizonColorDistance);

      //Fog on ocean
      //Shader.SetGlobalVector("_WaterZenith", fog.waterZenith);
      //Shader.SetGlobalVector("_WaterHorizon", fog.waterHorizon);

      //Shader.SetGlobalTexture("_FogTexture", fog.fogTexture);

     // Shader.SetGlobalFloat("_FogTextureBlend", fog.fogTextureBlend);

      /*
      Shader.SetGlobalVector("_Ustwo_Sun", new Vector4(sunIntensity, sunBackgroundIntensity,Mathf.Clamp01(1-sunBackgroundRadius), Mathf.Clamp01(1-sunRadius)));

      Shader.SetGlobalFloat("_Ustwo_SkyBackgroundIntensity", skyBackgroundIntensity);

      transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up)* Quaternion.AngleAxis(pitch, Vector3.right);
      */
    }
    
    /*
    [System.Serializable]
    public struct AmbientLight{
      public Color ambientSkyColor;
      public float ambientSkyIntensity;
      public Color ambientGroundColor;
      public float ambientGroundIntensity;

      public static AmbientLight operator +(AmbientLight a1, AmbientLight a2){
        AmbientLight a = new AmbientLight();
        a.ambientSkyColor = a1.ambientSkyColor + a2.ambientSkyColor;
        a.ambientSkyIntensity = a1.ambientSkyIntensity + a2.ambientSkyIntensity;
        a.ambientGroundColor = a1.ambientGroundColor + a2.ambientGroundColor;
        a.ambientGroundIntensity = a1.ambientGroundIntensity + a2.ambientGroundIntensity;
        return a;
      }
      public static AmbientLight operator *(float scaler, AmbientLight a1){
        AmbientLight a = new AmbientLight();
        a.ambientSkyColor = scaler *a1.ambientSkyColor;
        a.ambientSkyIntensity = scaler *a1.ambientSkyIntensity;
        a.ambientGroundColor = scaler *a1.ambientGroundColor;
        a.ambientGroundIntensity = scaler *a1.ambientGroundIntensity;
        return a;
      }
    }
    */

    [System.Serializable]
    public struct Fog{

      public float start;
      public float end;
      public Color horizonColor;
      public Color horizonColorDistance;
      public Color zenithColor;

      //public Color waterZenith;
      //public Color waterHorizon;
      //public Texture2D fogTexture;
      //[Range(0,1)]
      //public float fogTextureBlend;

      public static Fog operator +(Fog a1, Fog a2){
        Fog a = new Fog();
        a.start = a1.start + a2.start;
        a.end = a1.end + a2.end;
        a.horizonColor = a1.horizonColor + a2.horizonColor;
        a.horizonColorDistance = a1.horizonColorDistance + a2.horizonColorDistance;
        a.zenithColor = a1.zenithColor + a2.zenithColor;

        //a.waterZenith = a1.waterZenith + a2.waterZenith;
        //a.waterHorizon = a1.waterHorizon + a2.waterHorizon;
        return a;
      }
      public static Fog operator *(float scaler, Fog a1){
        Fog a = new Fog();
        a.start = scaler *a1.start;
        a.end = scaler *a1.end;
        a.horizonColor = scaler *a1.horizonColor;
        a.horizonColorDistance = scaler *a1.horizonColorDistance;
        a.zenithColor = scaler *a1.zenithColor;

        //a.waterZenith = scaler*a1.waterZenith;
        //a.waterHorizon = scaler*a1.waterHorizon;
        
        return a;
      }
    }
  }
}