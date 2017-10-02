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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// Used to reposition and scale a flat rectangular screen using pointer controls.
  /// Dragging is used for repositioning.
  /// Scrolling is used for scaling.
  public class MediaScreenController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler {
    [SerializeField]
    private float lerpSpeed = 8.0f;
  
    [SerializeField]
    [Range(1.0f, 6.0f)]
    private float maxLength = 3.0f;
  
    [SerializeField]
    [Range(0.5f, 2.0f)]
    private float aspectRatio = 1.77f;
  
    [SerializeField]
    [Range(0.1f, 5.0f)]
    private float minZoom = 0.5f;
  
    [SerializeField]
    [Range(0.1f, 5.0f)]
    private float maxZoom = 1.5f;
  
    [Range(0.1f, 5.0f)]
    private float scrollSensitivity = 1.0f;
  
    [SerializeField]
    private float minHeightAboveFloor = 0.5f;
  
    [SerializeField]
    private float minScreenCenterYPosition = 0.3f;
  
    [SerializeField]
    private bool enableScaling = true;
  
    [SerializeField]
    private bool enablePositioning = true;
  
    [SerializeField]
    private bool enableRotation = true;

    public float AspectRatio {
      get {
        return aspectRatio;
      }
      set {
        if (aspectRatio == value) {
          return;
        }
  
        aspectRatio = value;
        initialScreenScale = new Vector3(maxLength * aspectRatio, maxLength, 1.0f);
        desiredScreenScale = initialScreenScale * screenScale;
        if (enableScaling) {
          transform.localScale = desiredScreenScale;
        }
      }
    }

    public bool IsMovingScreen { get; private set; }

    public bool IsZoomingScreen { get; private set; }

    public bool IsManipulatingScreen {
      get {
        return IsMovingScreen || IsZoomingScreen;
      }
    }

    public bool IsInitialized { get; private set; }

    private Quaternion lastYawPitchRotation = Quaternion.identity;
    private Vector3 initialScreenScale;
    private bool didZoomScreenThisFrame = false;
  
    private Vector3 desiredScreenPos;
    private Vector3 desiredScreenScale;
  
    private static float screenScale = 1.0f;
    private static Quaternion lastFinalRotation = Quaternion.identity;

    public void OnBeginDrag(PointerEventData eventData) {
      IsMovingScreen = true;
      Quaternion yawPitchRotation = CalculateControllerYawPitchRotation(eventData);
      lastYawPitchRotation = yawPitchRotation;
    }

    public void OnDrag(PointerEventData eventData) {
      Quaternion yawPitchRotation = CalculateControllerYawPitchRotation(eventData);
      Quaternion yawPitchRotationDelta = yawPitchRotation * Quaternion.Inverse(lastYawPitchRotation);
      transform.parent.localRotation = yawPitchRotationDelta * transform.parent.localRotation;
      Vector3 eulerAngles = transform.parent.localRotation.eulerAngles;
      eulerAngles.z = 0.0f;
      lastFinalRotation = Quaternion.Euler(eulerAngles);
      lastYawPitchRotation = yawPitchRotation;
  
      if (enableRotation) {
        transform.parent.localRotation = lastFinalRotation;
      }
    }

    public void OnEndDrag(PointerEventData eventData) {
      StartCoroutine(StopMovingScreenAtEndOfFrame());
    }

    public void OnScroll(PointerEventData eventData) {
      if (IsMovingScreen || !enableScaling) {
        return;
      }
  
      float scaleDelta = eventData.scrollDelta.y / GvrPointerScrollInput.SCROLL_DELTA_MULTIPLIER;
      scaleDelta *= scrollSensitivity;
  
      screenScale = Mathf.Clamp(screenScale + scaleDelta, minZoom, maxZoom);
      desiredScreenScale = initialScreenScale * screenScale;
  
      float screenHeight = desiredScreenScale.y;
  
      float yPosition = CalculateScreenCenterYPosition(screenHeight);
      desiredScreenPos = transform.localPosition;
      desiredScreenPos.y = yPosition;
  
      IsZoomingScreen = true;
      didZoomScreenThisFrame = true;
    }

    void Awake() {
      SceneManager.sceneLoaded += OnSceneUnloaded;
    }

    void Start() {
      Init();
    }

    void OnEnable() {
      Init();
    }

    void OnDestroy() {
      SceneManager.sceneLoaded -= OnSceneUnloaded;
    }

    void Update() {
      if (GvrControllerInput.Recentered && enableRotation) {
        Quaternion currentRotation = transform.parent.localRotation;
        Vector3 eulerAngles = currentRotation.eulerAngles;
        eulerAngles.y = 0.0f;
        currentRotation = Quaternion.Euler(eulerAngles);
        transform.parent.localRotation = currentRotation;
      }
  
      if (enablePositioning) {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredScreenPos, Time.deltaTime * lerpSpeed);
      }
  
      if (enableScaling) {
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScreenScale, Time.deltaTime * lerpSpeed);
      }
    }

    void LateUpdate() {
      IsZoomingScreen = didZoomScreenThisFrame;
      didZoomScreenThisFrame = false;
    }

    private void Init() {
      if (transform.parent == null) {
        return;
      }
  
      // Scale
      initialScreenScale = new Vector3(maxLength * aspectRatio, maxLength, 1.0f);
      screenScale = Mathf.Clamp(screenScale, minZoom, maxZoom);
      desiredScreenScale = initialScreenScale * screenScale;
      if (enableScaling) {
        transform.localScale = desiredScreenScale;
      }
  
      // Position
      float yPosition = CalculateScreenCenterYPosition(desiredScreenScale.y);
      desiredScreenPos = transform.localPosition;
      desiredScreenPos.y = yPosition;
      if (enablePositioning) {
        transform.localPosition = desiredScreenPos;
      }
  
      if (enableRotation) {
        transform.parent.localRotation = lastFinalRotation;
      }
  
      IsInitialized = true;
    }

    private IEnumerator StopMovingScreenAtEndOfFrame() {
      yield return new WaitForEndOfFrame();
      IsMovingScreen = false;
    }

    private Quaternion CalculateControllerYawPitchRotation(PointerEventData eventData) {
      Vector3 pointerPos = eventData.pointerCurrentRaycast.worldPosition;
      if (pointerPos == Vector3.zero) {
        Transform pointerTransform = GvrPointerInputModule.Pointer.PointerTransform;
        pointerPos = pointerTransform.position +
          (pointerTransform.forward * GvrPointerInputModule.Pointer.MaxPointerDistance);
      }
      Vector3 controllerDir = pointerPos - Camera.main.transform.position;
      controllerDir.Normalize();
  
      Vector3 controllerDirWithoutY = controllerDir;
      controllerDirWithoutY.y = 0.0f;
      float horizontalDistance = Vector3.Distance(Vector3.forward, controllerDirWithoutY);
      float forwardHorizontalLength = 1.0f;
      float controllerHorizontalLength = new Vector2(controllerDir.x, controllerDir.z).magnitude;
      float horizontalAngle = Mathf.Acos(
                                ((forwardHorizontalLength * forwardHorizontalLength) +
                                (controllerHorizontalLength * controllerHorizontalLength) - (horizontalDistance * horizontalDistance)) /
                                (2 * forwardHorizontalLength * controllerHorizontalLength)) * Mathf.Rad2Deg;
      if (controllerDir.x < 0) {
        horizontalAngle = -horizontalAngle;
      }
  
      // The vertical logic calculates the angle between the controller-vector
      // without its Y component on the XZ plane and the controller vector with the
      // Y component
      float verticalDistance = Vector3.Distance(controllerDirWithoutY, controllerDir);
      float controllerDirWithoutYLength = controllerDirWithoutY.magnitude;
      float controllerDirLength = controllerDir.magnitude;
      float denominator = (2 * controllerDirWithoutYLength * controllerDirLength);
      float verticalAngle = 0.0f;
      if (denominator != 0.0f) {
        verticalAngle = Mathf.Acos(
          ((controllerDirWithoutYLength * controllerDirWithoutYLength) +
          (controllerDirLength * controllerDirLength) - (verticalDistance * verticalDistance)) /
          denominator) * Mathf.Rad2Deg;
      }
      if (controllerDir.y > 0) {
        verticalAngle = -verticalAngle;
      }
  
      Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
      Quaternion verticalRotation = Quaternion.AngleAxis(verticalAngle, Vector3.right);
      Quaternion yawPitchRotation = horizontalRotation * verticalRotation;
  
      return yawPitchRotation;
    }

    private float CalculateScreenCenterYPosition(float screenHeight) {
      // Figure out where the center of the screen should be such that it is at least
      // minHeightAboveFloor meters above the floor and the center of the screen
      // is at least minScreenCenterYPosition meters from the origin
      // vertically.
      float lowestAllowedYPositionBottom = minHeightAboveFloor - transform.parent.position.y;
      float lowestAllowedYPositionCenter = lowestAllowedYPositionBottom + screenHeight / 2.0f;
      float actualScreenCenterYPosition = minScreenCenterYPosition;
      if (lowestAllowedYPositionCenter > minScreenCenterYPosition) {
        actualScreenCenterYPosition = lowestAllowedYPositionCenter;
      }
      return actualScreenCenterYPosition;
    }

    private void OnSceneUnloaded(Scene scene, LoadSceneMode mode) {
      // TODO: This is a hack. MediaScreenController should be refactored to
      // persist the scale and rotation of the screen in a different way.
      screenScale = 1.0f;
      lastFinalRotation = Quaternion.identity;
    }
  }
}
