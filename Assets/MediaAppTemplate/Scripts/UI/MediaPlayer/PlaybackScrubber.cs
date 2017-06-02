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

  /// This script is a slider used to control the position of the video player.
  public class PlaybackScrubber : Slider, IPlaybackControl, IGvrPointerHoverHandler {
    public const string POSITION_LABEL_PROP_NAME = "positionLabel";
    public const string DURATION_LABEL_PROP_NAME = "durationLabel";
    public const string HOVER_HANDLE_PROP_NAME = "hoverHandle";
    public const string HOVER_LABEL_PROP_NAME = "hoverLabel";
    public const string STORYBOARD_PROP_NAME = "storyboardPrefab";

    public bool IsScrubbing { get; private set; }

    [SerializeField]
    private Text positionLabel;
  
    [SerializeField]
    private Text durationLabel;
  
    [SerializeField]
    private RectTransform hoverHandle;
  
    [SerializeField]
    private Text hoverLabel;
  
    [SerializeField]
    private GameObject storyboardPrefab;
  
    private float hoverValue = 0.0f;
  
    private BaseVideoPlayer videoPlayer;
    private BaseVideoPlayerStoryboard storyboardPlayer;

    public void Setup(BaseMediaPlayer player) {
      videoPlayer = player as BaseVideoPlayer;
      gameObject.SetActive(videoPlayer != null);
    }

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {
      base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {
      base.OnPointerUp(eventData);
  
      if (videoPlayer != null
          && videoPlayer.IsVideoLoadedAndDisplayed
          && hoverHandle.gameObject.activeSelf) {
        videoPlayer.CurrentPositionMilliseconds = (long)hoverValue;
      }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
      base.OnPointerEnter(eventData);
      hoverHandle.gameObject.SetActive(true);
      UpdateHoverHandle(eventData);
      IsScrubbing = true;
    }

    public override void OnPointerExit(PointerEventData eventData) {
      base.OnPointerExit(eventData);
      hoverHandle.gameObject.SetActive(false);
      IsScrubbing = false;
  
      if (storyboardPlayer != null) {
        GameObject.Destroy(storyboardPlayer.gameObject);
      }
    }

    public void OnGvrPointerHover(PointerEventData eventData) {
      UpdateHoverHandle(eventData);
    }

    protected override void Awake() {
      base.Awake();
      hoverHandle.gameObject.SetActive(false);
    }

    protected override void OnDisable() {
      base.OnDisable();
      if (hoverHandle.gameObject.activeSelf) {
        IsScrubbing = false;
        hoverHandle.gameObject.SetActive(false);
      }
  
      if (storyboardPlayer != null) {
        GameObject.Destroy(storyboardPlayer.gameObject);
      }
    }

    void Update() {
      if (videoPlayer == null || !videoPlayer.IsVideoLoadedAndDisplayed) {
        value = 0.0f;
        durationLabel.enabled = false;
        positionLabel.enabled = false;
        return;
      }
  
      durationLabel.enabled = true;
      positionLabel.enabled = true;
  
      if (maxValue != videoPlayer.DurationMilliseconds) {
        maxValue = videoPlayer.DurationMilliseconds;
        durationLabel.text = FormatTime(videoPlayer.DurationMilliseconds);
      }
  
      value = videoPlayer.CurrentPositionMilliseconds;
      positionLabel.text = FormatTime(videoPlayer.CurrentPositionMilliseconds);
    }

    private void UpdateHoverHandle(PointerEventData eventData) {
      RectTransform clickRect = hoverHandle.parent as RectTransform;
      if (clickRect == null) {
        return;
      }
  
      Vector2 localCursor;
      if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, eventData.enterEventCamera, out localCursor)) {
        return;
      }
      localCursor -= clickRect.rect.position;
  
      float normalVal = Mathf.Clamp01((localCursor).x / clickRect.rect.size.x);
      hoverValue = Mathf.Lerp(minValue, maxValue, normalVal);
  
      Vector2 anchorMin = Vector2.zero;
      Vector2 anchorMax = Vector2.one;
      anchorMin.x = anchorMax.x = normalVal;
      hoverHandle.anchorMin = anchorMin;
      hoverHandle.anchorMax = anchorMax;
      hoverLabel.text = FormatTime((long)hoverValue);
  
      CreateStoryboardPlayer();
      if (storyboardPlayer != null) {
        long previewPosition = (long)hoverValue;
        storyboardPlayer.PreviewPositionMilliseconds = previewPosition;
      }
    }

    private void CreateStoryboardPlayer() {
      if (storyboardPlayer != null) {
        return;
      }
  
      if (storyboardPrefab == null) {
        return;
      }
  
      if (videoPlayer == null) {
        return;
      }
  
      if (!videoPlayer.IsVideoLoadedAndDisplayed) {
        return;
      }
  
      GameObject storyboardObject = GameObject.Instantiate(storyboardPrefab);
      storyboardObject.transform.SetParent(hoverHandle, false);
      storyboardPlayer = storyboardObject.GetComponent<BaseVideoPlayerStoryboard>();
      storyboardPlayer.Initialize(videoPlayer.CurrentVideo, videoPlayer.DurationMilliseconds);
    }

    private string FormatTime(long ms) {
      int sec = ((int)(ms / 1000L));
      int mn = sec / 60;
      sec = sec % 60;
      int hr = mn / 60;
      mn = mn % 60;
      if (hr > 0) {
        return string.Format("{0}:{1:00}:{2:00}", hr, mn, sec);
      }
      return string.Format("{0}:{1:00}", mn, sec);
    }
  }
}
