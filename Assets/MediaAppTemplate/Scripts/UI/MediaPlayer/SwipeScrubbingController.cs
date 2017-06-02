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

  /// This script is used to control the position of the video player by using swipe gestures.
  public class SwipeScrubbingController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField]
    private long pausedSpeedMilliseconds;
  
    [SerializeField]
    private long playForwardSpeedMilliseconds;
  
    [SerializeField]
    private long playBackwardSpeedMilliseconds;
  
    private BaseVideoPlayer videoPlayer;
    private bool isHovering = false;

    void Awake() {
      videoPlayer = GetComponent<BaseVideoPlayer>();
    }

    void Update() {
      if (videoPlayer == null || !videoPlayer.IsVideoLoadedAndDisplayed) {
        return;
      }
  
      if (isHovering) {
        return;
      }
  
      foreach (GestureDetector.Gesture gesture in GestureManager.Instance.GestureList) {
        if (gesture.type == GestureDetector.Type.SWIPE) {
          long directionCoeff = 0;
          long playSpeed = 0;
          if (gesture.direction == GestureDetector.Direction.RIGHT) {
            directionCoeff = 1;
            playSpeed = playForwardSpeedMilliseconds;
          } else if (gesture.direction == GestureDetector.Direction.LEFT) {
            directionCoeff = -1;
            playSpeed = playBackwardSpeedMilliseconds;
          }
  
          long speed = videoPlayer.Paused ? pausedSpeedMilliseconds : playSpeed;
          long positionDelta = speed * directionCoeff;
          if (positionDelta != 0) {
            long newPosition = videoPlayer.CurrentPositionMilliseconds;
            newPosition += positionDelta;
            if (newPosition < 0) {
              newPosition = 0;
            } else if (newPosition > videoPlayer.DurationMilliseconds) {
              newPosition = videoPlayer.DurationMilliseconds;
            }
  
            videoPlayer.CurrentPositionMilliseconds = newPosition;
          }
        }
      }
    }

    public void OnPointerEnter(PointerEventData eventData) {
      isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      isHovering = false;
    }
  }
}
