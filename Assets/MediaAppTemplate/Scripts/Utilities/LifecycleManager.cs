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
using System.Collections.Generic;
using System.Linq;
using System;

namespace Daydream.MediaAppTemplate {

  /// Provides a way to queue functions to be called on Update in the main thread.
  /// Also provides a way to get a callback on Update. Useful for scripts that want to
  /// respond to Update events when they are on an inactive GameObject.
  public class LifecycleManager : MonoBehaviour {
    private static LifecycleManager instance;

    public event Action OnUpdate;
    public event Action OnLateUpdate;

    public static LifecycleManager Instance {
      get {
        return instance;
      }
    }

    private List<Action> queuedOnUpdateActions = new List<Action>();

    public void QueueOnUpdateAction(Action action) {
      queuedOnUpdateActions.Add(action);
    }

    void Awake() {
      if (instance != null) {
        Debug.LogError("Cannot have multiple instances of Lifecycle Manager.");
      }
      instance = this;
    }

    void Update() {
      ExecuteOnUpdateActions();
  
      if (OnUpdate != null) {
        OnUpdate();
      }
    }

    void LateUpdate() {
      if (OnLateUpdate != null) {
        OnLateUpdate();
      }
    }

    private void ExecuteOnUpdateActions() {
      if (queuedOnUpdateActions.Count == 0) {
        return;
      }
  
      Action[] onUpdateActions = queuedOnUpdateActions.ToArray();
      queuedOnUpdateActions.Clear();
  
      foreach (Action action in onUpdateActions) {
        if (action != null) {
          action();
        }
      }
    }
  }
}
