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
using DaydreamElements.Tunneling;

namespace Daydream.MediaAppTemplate {

  /// Transitions a TunnelingVignette on/off for media player use case
  [RequireComponent(typeof(TunnelingVignette))]
  public class MediaPlayerVignetteController : MonoBehaviour {
    [Tooltip("Speed that the vignette fades in and out.")]
    public float fadeSpeed = 16.0f;
  
    [Tooltip("The minimum alpha that the vignette must be at to be considered ready.")]
    [Range(0.0f, 1.0f)]
    public float alphaReadyThreshold = 0.7f;
  
    [SerializeField]
    private float fov = 30.0f;
  
    private TunnelingVignette vignette;
    private bool showVignette = false;

    void Awake() {
      vignette = GetComponent<TunnelingVignette>();
      MediaPlayerEventDispatcher.OnSphericalScreenBeginDrag += OnSphericalScreenBeginDrag;
      MediaPlayerEventDispatcher.OnSphericalScreenEndDrag += OnSphericalScreenEndDrag;
    }

    void OnDestroy() {
      MediaPlayerEventDispatcher.OnSphericalScreenBeginDrag -= OnSphericalScreenBeginDrag;
      MediaPlayerEventDispatcher.OnSphericalScreenEndDrag -= OnSphericalScreenEndDrag;
    }

    void Update() {
      UpdateAlpha();
    }

    private void OnSphericalScreenEndDrag() {
      SetVignetteShown(false);
    }

    private void OnSphericalScreenBeginDrag() {
      SetVignetteShown(true);
    }

    private void SetVignetteShown(bool shown) {
      if (shown) {
        vignette.CurrentFOV = fov;
      } else {
        vignette.CurrentFOV = TunnelingVignette.MAX_FOV;
      }
  
      showVignette = shown;
    }

    private void UpdateAlpha() {
      float targetAlpha = showVignette ? 1.0f : 0.0f;
      if (vignette.Alpha == targetAlpha) {
        return;
      }
  
      vignette.Alpha = Mathf.Lerp(vignette.Alpha, targetAlpha, Time.deltaTime * fadeSpeed);
  
      if (Mathf.Abs(vignette.Alpha - targetAlpha) < 0.01f) {
        vignette.Alpha = targetAlpha;
      }
    }
  }
}
