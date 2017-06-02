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

  /// This script is a button used to play/pause the video player.
  public class PlaybackPlayPause : MonoBehaviour, IPlaybackControl, IPointerClickHandler {
    [SerializeField]
    private Sprite playSprite;

    [SerializeField]
    private Sprite pauseSprite;

    [SerializeField]
    private Image image;

    private BaseVideoPlayer videoPlayer;

    public void Setup(BaseMediaPlayer player) {
      videoPlayer = player as BaseVideoPlayer;
      gameObject.SetActive(videoPlayer != null);
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (videoPlayer == null || !videoPlayer.IsVideoLoadedAndDisplayed) {
        return;
      }

      videoPlayer.Paused = !videoPlayer.Paused;
    }

    void Update() {
      if (videoPlayer == null) {
        return;
      }

      if (videoPlayer.IsPlaying) {
        image.sprite = pauseSprite;
      } else {
        image.sprite = playSprite;
      }
    }
  }
}
