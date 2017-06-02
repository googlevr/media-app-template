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
using UnityEngine.EventSystems;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script must be on the root object of the PlaybackControls prefab used by the
  /// _PlaybackControlsManager_. It all children with _IPlaybackControl_ scripts access to
  /// the video player.
  [RequireComponent(typeof(RectTransform))]
  public class PlaybackControls : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField]
    private RectTransform bottom;
  
    [SerializeField]
    private RectTransform top;
  
    [SerializeField]
    [Range(0.5f, 5.0f)]
    private float distanceFromMedia = 2.5f;
  
    public Vector2 defaultSize = new Vector2(400.0f, 400.0f);
  
    private RectTransform rectTransform;
    private BaseMediaPlayer mediaPlayer;
    private bool hasPositionedScreen;
    private MediaScreenController screenController;

    public bool IsPointerHovering { get; private set; }

    public void Setup(BaseMediaPlayer player) {
      mediaPlayer = player;
      mediaPlayer.OnMediaScreenChanged += OnMediaScreenChanged;
  
      IPlaybackControl[] controls = GetComponentsInChildren<IPlaybackControl>(true);
      foreach (IPlaybackControl control in controls) {
        control.Setup(player);
      }
  
      RefreshLayout();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      IsPointerHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      IsPointerHovering = false;
    }

    void Awake() {
      rectTransform = GetComponent<RectTransform>();
      Assert.IsNotNull(bottom);
      Assert.IsNotNull(top);
    }

    void OnDestroy() {
      if (mediaPlayer != null) {
        mediaPlayer.OnMediaScreenChanged -= OnMediaScreenChanged;
      }
    }

    void OnDisable() {
      IsPointerHovering = false;
    }

    void LateUpdate() {
      RefreshLayout();
    }

    private void RefreshLayout() {
      if (mediaPlayer == null) {
        return;
      }
  
      if (mediaPlayer.MediaScreen == null && hasPositionedScreen) {
        return;
      }
  
      if (screenController != null && !screenController.IsInitialized && hasPositionedScreen) {
        return;
      }
  
      // Position the playback controls
      Vector3 localMediaCenter = mediaPlayer.LocalCenter;
      Vector3 worldMediaCenter = mediaPlayer.Center;
      Vector3 direction;
      if (localMediaCenter != Vector3.zero) {
        direction = Camera.main.transform.position - worldMediaCenter;
        direction.Normalize();
      } else {
        direction = Vector3.forward;
      }
  
      Vector3 diff = direction * distanceFromMedia;
      Vector3 pos = worldMediaCenter + diff;
      pos = rectTransform.parent.InverseTransformPoint(pos);
      rectTransform.anchoredPosition3D = pos;
  
      // Set the size of the playback controls
      // Adjusted by the difference in position between the playback controls and the media
      // So that the playback controls are positioned correctly relative to the screen
      // from the perspective of the camera.
      if (localMediaCenter != Vector3.zero) {
        float screenMagnitude = localMediaCenter.magnitude;
        float diffMagnitude = diff.magnitude;
        float inverseDiffMagnitude = screenMagnitude - diffMagnitude;
        float ratio = inverseDiffMagnitude / screenMagnitude;
  
        Vector2 scale = rectTransform.localScale;
        Vector2 size = mediaPlayer.LocalSize;
        size.x /= scale.x;
        size.y /= scale.y;
        size *= ratio;
        rectTransform.sizeDelta = size;
      } else {
        rectTransform.sizeDelta = defaultSize;
      }
  
      bottom.anchoredPosition3D = Vector3.zero;
      top.anchoredPosition3D = Vector3.zero;
  
      hasPositionedScreen = true;
    }

    private void OnMediaScreenChanged(Renderer mediaScreen) {
      if (mediaScreen != null) {
        screenController = mediaScreen.GetComponent<MediaScreenController>();
      } else {
        screenController = null;
      }
    }
  }
}
