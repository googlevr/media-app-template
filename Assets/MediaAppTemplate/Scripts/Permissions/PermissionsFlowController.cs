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

  /// This script is used to request the permissions that are required for the
  /// Media App Template and then progress to the next state of the app after
  /// permissions have been granted.
  public class PermissionsFlowController : MonoBehaviour {
#if UNITY_ANDROID && !UNITY_EDITOR
    private static string[] PERMISSIONS = { "android.permission.READ_EXTERNAL_STORAGE" };
  #endif
  
    [SerializeField]
    private GameObject postPermissionsGrantedPrefab;
  
    private GameObject postPermissionsGrantedObject;

    public bool NeedsPermissions() {
      #if UNITY_ANDROID && !UNITY_EDITOR
      bool[] grantedStatuses = GvrPermissionsRequester.Instance.HasPermissionsGranted(PERMISSIONS);
      foreach (bool granted in grantedStatuses) {
        if (!granted) {
          return true;
        }
      }
      #endif
      return false;
    }

    public void RequestPermissions() {
      #if UNITY_ANDROID && !UNITY_EDITOR
      GvrPermissionsRequester.Instance.RequestPermissions(PERMISSIONS, OnRequestPermissions);
      #endif
    }

    private void OnRequestPermissions(GvrPermissionsRequester.PermissionStatus[] statuses) {
      #if UNITY_ANDROID && !UNITY_EDITOR
      bool areAllPermissionsGranted = true;
      foreach (GvrPermissionsRequester.PermissionStatus status in statuses) {
        if (!status.Granted) {
          areAllPermissionsGranted = false;
        }
      }
  
      if (areAllPermissionsGranted) {
        PostPermissionsGranted();
      }
      #endif
    }

    void Awake() {
      if (!NeedsPermissions()) {
        PostPermissionsGranted();
      }
    }

    private void PostPermissionsGranted() {
      if (postPermissionsGrantedObject != null) {
        return;
      }
  
      postPermissionsGrantedObject = GameObject.Instantiate(postPermissionsGrantedPrefab);
      Destroy(gameObject);
    }
  }
}
