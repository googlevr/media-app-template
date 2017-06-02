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
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// Toggles the active status of a target GameObject
  /// based on the state of a UI Toggle.
  [RequireComponent(typeof(Toggle))]
  public class GameObjectToggle : MonoBehaviour {
    [SerializeField]
    private GameObject objectToToggle;
  
    private Toggle toggle;

    void Awake() {
      toggle = GetComponent<Toggle>();
      Assert.IsNotNull(toggle);
      toggle.onValueChanged.AddListener(OnValueChanged);
    }

    void Start() {
      OnValueChanged(toggle.isOn);
    }

    void OnDestroy() {
      if (toggle != null) {
        toggle.onValueChanged.RemoveListener(OnValueChanged);
      }
    }

    void Update() {
      // If the active status of the object to toggle
      // changes from some other source, make sure the toggle gets
      // updated to reflect the active status of the object.
      if (objectToToggle.activeSelf != toggle.isOn) {
        toggle.isOn = objectToToggle.activeSelf;
      }
    }

    private void OnValueChanged(bool value) {
      objectToToggle.SetActive(value);
    }
  }
}
