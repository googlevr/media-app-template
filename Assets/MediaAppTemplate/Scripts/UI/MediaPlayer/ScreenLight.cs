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

using UnityEngine;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script is used to visualize light from the video screen.
  [RequireComponent(typeof(Light))]
  public class ScreenLight : MonoBehaviour {
    [SerializeField]
    private AverageColorCalculator averageColorCalculator;
  
    [SerializeField]
    private float colorMultiplier = 1.2f;
  
    [SerializeField]
    private float minIntensity = 1.0f;
  
    [SerializeField]
    private float maxIntensity = 3.0f;
  
    [SerializeField]
    private float lerpSpeed = 16.0f;
  
    [SerializeField]
    private float heightThresholdMax = 0.2f;
  
    [SerializeField]
    private float heightThresholdMin = 0.0f;
  
    [SerializeField]
    private Transform bottomOfScreen;
  
    private Light screenLight;
    float currentIntensity = 0.0f;

    void Awake() {
      screenLight = GetComponent<Light>();
      screenLight.color = Color.black;
      screenLight.intensity = 0.0f;
    }

    void Update() {
      if (averageColorCalculator.AverageColorAvailable) {
        Color desiredColor = averageColorCalculator.AverageColor * colorMultiplier;
        Color color = screenLight.color;
        color = Color.Lerp(color, desiredColor, Time.deltaTime * lerpSpeed);
        screenLight.color = color;
  
        float brightness = color.maxColorComponent;
        float desiredIntensity = minIntensity + ((maxIntensity - minIntensity) * brightness);
  
        float y = bottomOfScreen.position.y;
        float ratio = Mathf.Clamp01((y - heightThresholdMin) / (heightThresholdMax - heightThresholdMin));
        desiredIntensity *= ratio;
  
  
        currentIntensity = Mathf.Lerp(currentIntensity, desiredIntensity, Time.deltaTime * lerpSpeed);
      } else {
        currentIntensity = 0.0f;
      }
  
      screenLight.intensity = currentIntensity;
    }
  }
}
