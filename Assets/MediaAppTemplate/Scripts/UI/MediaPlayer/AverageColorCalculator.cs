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

  /// This script is used to calculate the average color of the main texture
  /// rendererd by the targetRenderer
  public class AverageColorCalculator : MonoBehaviour {
    [SerializeField]
    private Renderer targetRenderer;
  
    [SerializeField]
    [Range(1, 10)]
    private int frameInterval = 3;
  
    private Texture2D targetTexture;
    private const int TEXTURE_SIZE = 4;
    private RenderTexture renderTexture = null;

  
    public bool AverageColorAvailable { get; private set; }

    public Color AverageColor { get; private set; }

    void Awake() {
      targetTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);
    }

    void OnEnable() {
      StartCoroutine(CalculateAverageColorLoop());
    }

    private IEnumerator CalculateAverageColorLoop() {
      while (true) {
  
        // Blit the texture to a tiny render texture.
        Blit();
  
        // Wait a few frames before reading the pixels to avoid pipeline stalls
        // and reduce the amount of work done on a single frame.
        for (int i = 0; i < frameInterval; i++) {
          yield return new WaitForEndOfFrame();
        }
  
        CalculateAverageColor();
  
      }
    }

    private void Blit() {
      if (renderTexture != null) {
        return;
      }
  
      renderTexture = RenderTexture.GetTemporary(TEXTURE_SIZE, TEXTURE_SIZE, 0);
      Graphics.Blit(targetRenderer.material.mainTexture, renderTexture, targetRenderer.material);
    }

    private void CalculateAverageColor() {
      if (renderTexture == null) {
        return;
      }
  
      RenderTexture.active = renderTexture;
      targetTexture.ReadPixels(new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), 0, 0, false);
      RenderTexture.ReleaseTemporary(renderTexture);
      renderTexture = null;
      Color32[] pixels = targetTexture.GetPixels32();
  
      float r = 0.0f;
      float g = 0.0f;
      float b = 0.0f;
      int numPixels = pixels.Length;
  
      for (int i = 0; i < pixels.Length; i++) {
        Color pixel = pixels[i];
        r += pixel.r;
        g += pixel.g;
        b += pixel.b;
      }
  
      AverageColor = new Color(r / numPixels, g / numPixels, b / numPixels);
      AverageColorAvailable = true;
    }
  }
}
