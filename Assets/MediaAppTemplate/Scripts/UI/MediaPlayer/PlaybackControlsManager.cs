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
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script manages when the _PlaybackControls_ are open and closed.
  /// It also provides the _PlaybackControls_ access to the video player.
  public class PlaybackControlsManager : MonoBehaviour {
    [SerializeField]
    private GameObject playbackControlsPrefab;
  
    [SerializeField]
    private BaseMediaPlayer player;
  
    [SerializeField]
    private float closeOnIdleDelaySeconds = 5.0f;
  
    private PlaybackControls playbackControls;
    private Coroutine closeAfterDelayCoroutine;

    public bool ArePlaybackControlsOpen {
      get {
        if (playbackControls == null) {
          return false;
        }
  
        return playbackControls.gameObject.activeSelf;
      }
    }

    public void SetPlaybackControlsOpen(bool open) {
      if (open == ArePlaybackControlsOpen) {
        return;
      }
  
      CreatePlaybackControls();
      playbackControls.gameObject.SetActive(open);
    }

    void Start() {
      CreatePlaybackControls();
      MediaPlayerEventDispatcher.OnSphericalScreenBeginDrag += OnSphericalScreenBeginDrag;
    }

    void OnDestroy() {
      MediaPlayerEventDispatcher.OnSphericalScreenBeginDrag -= OnSphericalScreenBeginDrag;
    }

    void Update() {
      if (playbackControls == null) {
        return;
      }
  
      bool isManipulatingScreen = ConditionsManager.Instance.IsConditionTrue(ManipulatingScreenCondition.IDENTIFIER);
      if (isManipulatingScreen) {
        return;
      }
  
      if (GvrControllerInput.ClickButtonUp && !playbackControls.IsPointerHovering) {
        TogglePlaybackControls();
      }
  
      bool isControllerIdle = ConditionsManager.Instance.IsConditionTrue(ControllerIdleCondition.IDENTIFIER);
      bool shouldAutoCloseControls = isControllerIdle && !playbackControls.IsPointerHovering;
      if (shouldAutoCloseControls && closeAfterDelayCoroutine == null) {
        closeAfterDelayCoroutine = StartCoroutine(CloseAfterDelay(closeOnIdleDelaySeconds));
      }
  
      if (!shouldAutoCloseControls && closeAfterDelayCoroutine != null) {
        StopCoroutine(closeAfterDelayCoroutine);
        closeAfterDelayCoroutine = null;
      }
  
      if (GvrControllerInput.AppButtonUp) {
        MediaPlayerEventDispatcher.RaiseExitMedia();
      }
    }

    private void CreatePlaybackControls() {
      if (playbackControls != null) {
        return;
      }
  
      GameObject controlsObject = GameObject.Instantiate(playbackControlsPrefab);
      controlsObject.transform.SetParent(player.transform, false);
      playbackControls = controlsObject.GetComponent<PlaybackControls>();
      playbackControls.Setup(player);
      controlsObject.SetActive(false);
    }

    private void TogglePlaybackControls() {
      if (playbackControls == null) {
        Debug.LogError("Cannot Toggle Playback Controls, they haven't been created.");
        return;
      }
  
      SetPlaybackControlsOpen(!ArePlaybackControlsOpen);
    }

    private IEnumerator CloseAfterDelay(float delaySeconds) {
      yield return new WaitForSeconds(delaySeconds);
      SetPlaybackControlsOpen(false);
    }

    private void OnSphericalScreenBeginDrag() {
      SetPlaybackControlsOpen(false);
    }
  }
}
