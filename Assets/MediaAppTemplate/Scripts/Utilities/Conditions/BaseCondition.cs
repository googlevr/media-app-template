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

  /// Abstract class used to register a condition with the _ConditionManager_
  public abstract class BaseCondition : MonoBehaviour {
    public abstract string ConditionIdentifier { get; }

    public abstract bool CheckCondition();

    void OnEnable() {
      if (ConditionsManager.Instance != null) {
        ConditionsManager.Instance.RegisterCondition(this);
      }
    }
  
    // This is necessary because when a scene is first loaded
    // The conditions manager will likely be null when OnEnable is called.
    // The OnEnable method is kept so that the condition will still be re-registered if
    // this script is disabled and then re-enabled.
    void Start() {
      if (ConditionsManager.Instance != null) {
        ConditionsManager.Instance.RegisterCondition(this);
      }
    }

    void OnDisable() {
      if (ConditionsManager.Instance != null) {
        ConditionsManager.Instance.UnregisterCondition(this);
      }
    }
  }
}
