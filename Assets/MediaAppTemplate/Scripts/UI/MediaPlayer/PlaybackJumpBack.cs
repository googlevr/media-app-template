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

  /// This script is a button used to jump the position of the video player 
  /// backwards by jumpAmountMilliseconds.
  public class PlaybackJumpBack : MonoBehaviour, IPlaybackControl, IPointerClickHandler {
    private BaseVideoPlayer videoPlayer;
  
    [SerializeField]
    private long jumpAmountMilliseconds;

    public void Setup(BaseMediaPlayer player) {
      videoPlayer = player as BaseVideoPlayer;
      gameObject.SetActive(videoPlayer != null);
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (videoPlayer == null || !videoPlayer.IsVideoLoadedAndDisplayed) {
        return;
      }
  
      long newPosition = videoPlayer.CurrentPositionMilliseconds - jumpAmountMilliseconds;
      if (newPosition < 0) {
        newPosition = 0;
      }
      videoPlayer.CurrentPositionMilliseconds = newPosition;
    }
  }
}
