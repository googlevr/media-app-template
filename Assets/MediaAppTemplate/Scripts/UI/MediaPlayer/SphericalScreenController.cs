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
using UnityEngine.EventSystems;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script is used to drag a spherical screen so that all parts of the sphere
  /// can be seen without requiring the player to use a swivel chair.
  public class SphericalScreenController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public bool IsDragging { get; private set; }

    public void OnBeginDrag(PointerEventData eventData) {
      IsDragging = true;
      MediaPlayerEventDispatcher.RaiseSphericalScreenBeginDrag();
    }

    public void OnDrag(PointerEventData eventData) {
      float dragAmount = eventData.delta.x * Mathf.Rad2Deg * -1.0f;
      Quaternion dragRotation = Quaternion.AngleAxis(dragAmount, Vector3.up);
  
      Quaternion currentRotation = transform.localRotation;
      currentRotation = currentRotation * dragRotation;
      transform.localRotation = currentRotation;
    }

    public void OnEndDrag(PointerEventData eventData) {
      StartCoroutine(StopDraggingAtEndOfFrame());
    }

    void Start() {
      transform.parent.localRotation = Quaternion.identity;
    }

    private IEnumerator StopDraggingAtEndOfFrame() {
      yield return new WaitForEndOfFrame();
      IsDragging = false;
      MediaPlayerEventDispatcher.RaiseSphericalScreenEndDrag();
    }
  }
}
