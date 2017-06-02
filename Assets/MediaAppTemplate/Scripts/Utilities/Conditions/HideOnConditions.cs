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
using UnityEngine.UI;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script is used to hide a GameObject based on conditions in the
  /// _ConditionsManager_. Uses a _ConditionChecker_ to evaluate the conditions.
  public class HideOnConditions : MonoBehaviour {
    [SerializeField]
    private ConditionsChecker conditions;
  
    private Graphic targetGraphic;
    private Renderer targetRenderer;

    void Awake() {
      targetRenderer = GetComponent<Renderer>();
      targetGraphic = GetComponent<Graphic>();
    }

    void Start() {
      LifecycleManager.Instance.OnLateUpdate += RefreshHidden;
    }

    void OnDestroy() {
      if (ConditionsManager.Instance != null) {
        LifecycleManager.Instance.OnLateUpdate -= RefreshHidden;
      }
    }

    private void RefreshHidden() {
      SetHidden(conditions.Evaluate());
    }

    private void SetHidden(bool hidden) {
      bool foundVisual = false;
      if (targetRenderer != null) {
        targetRenderer.enabled = !hidden;
        foundVisual = true;
      }
      if (targetGraphic != null) {
        targetGraphic.enabled = !hidden;
        foundVisual = true;
      }
  
      if (!foundVisual) {
        gameObject.SetActive(!hidden);
      }
    }
  }
}
