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

  /// UI for manually changing the Stereo Projection format of a MediaPlayer.
  /// Also saves the manually selected format for this file
  /// so that it persists between sessions.
  public class PlaybackFormatSettings : MonoBehaviour, IPlaybackControl {
    private BaseMediaPlayer mediaPlayer;
    private BaseVideoPlayer videoPlayer;

    [SerializeField]
    private Toggle monoToggle;

    [SerializeField]
    private Toggle leftRightToggle;

    [SerializeField]
    private Toggle topBottomToggle;

    [SerializeField]
    private Toggle flatToggle;

    [SerializeField]
    private Toggle projection180Toggle;

    [SerializeField]
    private Toggle projection360Toggle;

    public void Setup(BaseMediaPlayer player) {
      mediaPlayer = player;
      videoPlayer = mediaPlayer as BaseVideoPlayer;

      // Stereo
      Assert.IsNotNull(monoToggle);
      monoToggle.onValueChanged.AddListener(OnMonoToggleChanged);
      Assert.IsNotNull(leftRightToggle);
      leftRightToggle.onValueChanged.AddListener(OnLeftRightToggleChanged);
      Assert.IsNotNull(topBottomToggle);
      topBottomToggle.onValueChanged.AddListener(OnTopBottomToggleChanged);

      // Make sure all Stereo toggles are in the same toggle group.
      Assert.IsTrue(monoToggle.group != null
        && monoToggle.group == leftRightToggle.group
        && monoToggle.group == topBottomToggle.group);

      // Projection
      Assert.IsNotNull(flatToggle);
      flatToggle.onValueChanged.AddListener(OnFlatToggleChanged);
      Assert.IsNotNull(projection180Toggle);
      projection180Toggle.onValueChanged.AddListener(OnProjection180ToggleChanged);
      Assert.IsNotNull(projection360Toggle);
      projection360Toggle.onValueChanged.AddListener(OnProjection360ToggleChanged);

      // Make sure all Projection toggles are in the same toggle group.
      Assert.IsTrue(flatToggle.group != null
        && flatToggle.group == projection180Toggle.group
        && flatToggle.group == projection360Toggle.group);

      SetStereoModeToggle(mediaPlayer.CurrentStereoMode);
      SetProjectionModeToggle(mediaPlayer.CurrentProjectionMode);
      SetTogglesInteractable(mediaPlayer.MediaScreen != null);

      mediaPlayer.OnStereoModeChanged += SetStereoModeToggle;
      mediaPlayer.OnProjectionModeChanged += SetProjectionModeToggle;
      mediaPlayer.OnMediaScreenChanged += OnMediaScreenChanged;
    }

    void OnEnable() {
      // If a video is playing while this menu is opened,
      // pause the video.
      if (videoPlayer != null) {
        videoPlayer.Paused = true;
      }
    }

    void OnDisable() {
      if (gameObject.activeSelf) {
        gameObject.SetActive(false);
      }
    }

    void Update() {
      // If a video is played while this menu is open,
      // close the menu.
      if (videoPlayer != null && videoPlayer.IsPlaying) {
        gameObject.SetActive(false);
      }
    }

    void OnDestroy() {
      if (mediaPlayer != null) {
        mediaPlayer.OnStereoModeChanged -= SetStereoModeToggle;
        mediaPlayer.OnProjectionModeChanged -= SetProjectionModeToggle;
        mediaPlayer.OnMediaScreenChanged -= OnMediaScreenChanged;
      }
    }

    private void OnMediaScreenChanged(Renderer screen) {
      SetTogglesInteractable(screen != null);
    }

    private void SetStereoModeToggle(BaseMediaPlayer.StereoMode stereoMode) {
      monoToggle.isOn = false;
      leftRightToggle.isOn = false;
      topBottomToggle.isOn = false;

      switch (stereoMode) {
        case BaseMediaPlayer.StereoMode.Mono:
          monoToggle.isOn = true;
          break;
        case BaseMediaPlayer.StereoMode.LeftRight:
          leftRightToggle.isOn = true;
          break;
        case BaseMediaPlayer.StereoMode.TopBottom:
          topBottomToggle.isOn = true;
          break;
        default:
          monoToggle.isOn = true;
          break;
      }
    }

    private void SetStereoMode(BaseMediaPlayer.StereoMode stereoMode) {
      if (stereoMode == mediaPlayer.CurrentStereoMode) {
        return;
      }

      if (mediaPlayer.MediaScreen == null) {
        return;
      }

      mediaPlayer.CurrentStereoMode = stereoMode;
      float frameAspectRatio =
        ImageBasedProjectionDetectorHelpers.CalculateFrameAspectRatio(mediaPlayer.RawAspectRatio, stereoMode);
      mediaPlayer.CurrentAspectRatio = frameAspectRatio;

      mediaPlayer.SaveCurrentFormat();
    }

    private void SetProjectionModeToggle(BaseMediaPlayer.ProjectionMode projectionMode) {
      flatToggle.isOn = false;
      projection180Toggle.isOn = false;
      projection360Toggle.isOn = false;

      switch (projectionMode) {
        case BaseMediaPlayer.ProjectionMode.Flat:
          flatToggle.isOn = true;
          break;
        case BaseMediaPlayer.ProjectionMode.Projection180:
          projection180Toggle.isOn = true;
          break;
        case BaseMediaPlayer.ProjectionMode.Projection360:
          projection360Toggle.isOn = true;
          break;
        default:
          flatToggle.isOn = true;
          break;
      }
    }

    private void SetProjectionMode(BaseMediaPlayer.ProjectionMode projectionMode) {
      if (projectionMode == mediaPlayer.CurrentProjectionMode) {
        return;
      }

      mediaPlayer.CurrentProjectionMode = projectionMode;
      mediaPlayer.SaveCurrentFormat();
    }

    private void OnMonoToggleChanged(bool isOn) {
      if (isOn) {
        SetStereoMode(BaseMediaPlayer.StereoMode.Mono);
      }
    }

    private void OnLeftRightToggleChanged(bool isOn) {
      if (isOn) {
        SetStereoMode(BaseMediaPlayer.StereoMode.LeftRight);
      }
    }

    private void OnTopBottomToggleChanged(bool isOn) {
      if (isOn) {
        SetStereoMode(BaseMediaPlayer.StereoMode.TopBottom);
      }
    }

    private void OnFlatToggleChanged(bool isOn) {
      if (isOn) {
        SetProjectionMode(BaseMediaPlayer.ProjectionMode.Flat);
      }
    }

    private void OnProjection180ToggleChanged(bool isOn) {
      if (isOn) {
        SetProjectionMode(BaseMediaPlayer.ProjectionMode.Projection180);
      }
    }

    private void OnProjection360ToggleChanged(bool isOn) {
      if (isOn) {
        SetProjectionMode(BaseMediaPlayer.ProjectionMode.Projection360);
      }
    }

    private void SetTogglesInteractable(bool interactable) {
      monoToggle.interactable = interactable;
      leftRightToggle.interactable = interactable;
      topBottomToggle.interactable = interactable;

      flatToggle.interactable = interactable;
      projection180Toggle.interactable = interactable;
      projection360Toggle.interactable = interactable;
    }
  }
}
