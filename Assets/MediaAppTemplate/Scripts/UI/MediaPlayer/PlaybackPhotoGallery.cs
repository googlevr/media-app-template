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

  /// This script is a button used to automatically move to the next photo
  /// after a delay so that photos can be viewed in a sequence automatically.
  public class PlaybackPhotoGallery : MonoBehaviour, IPlaybackControl, IPointerClickHandler {
    [SerializeField]
    private Sprite playSprite;
  
    [SerializeField]
    private Sprite pauseSprite;
  
    [SerializeField]
    private Image image;
  
    [SerializeField]
    private Image timer;
  
    [SerializeField]
    private float timeUntilNextPhotoSeconds = 5.0f;
  
    private bool playing = false;
    private Coroutine nextFileCoroutine;

    public void Setup(BaseMediaPlayer player) {
      BasePhotoPlayer photoPlayer = player as BasePhotoPlayer;
      gameObject.SetActive(photoPlayer != null);
      timer.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
      playing = !playing;
  
      StopTimer();
  
      if (playing) {
        image.sprite = pauseSprite;
        StartTimer();
      } else {
        image.sprite = playSprite;
      }
    }

    void Awake() {
      MediaPlayerEventDispatcher.OnNextFile += OnFileChanged;
      MediaPlayerEventDispatcher.OnPreviousFile += OnFileChanged;
    }

    void OnDestroy() {
      StopTimer();
      MediaPlayerEventDispatcher.OnNextFile -= OnFileChanged;
      MediaPlayerEventDispatcher.OnPreviousFile -= OnFileChanged;
    }

    private void OnFileChanged() {
      if (playing) {
        StartTimer();
      } else {
        StopTimer();
      }
    }

    private void StartTimer() {
      StopTimer();
      nextFileCoroutine = LifecycleManager.Instance.StartCoroutine(NextFileAfterDelay());
    }

    private void StopTimer() {
      if (nextFileCoroutine != null && LifecycleManager.Instance != null) {
        LifecycleManager.Instance.StopCoroutine(nextFileCoroutine);
        nextFileCoroutine = null;
        timer.enabled = false;
      }
    }

    private IEnumerator NextFileAfterDelay() {
      while (true) {
        float timeRemainingSeconds = timeUntilNextPhotoSeconds;
  
        while (timeRemainingSeconds > 0.0f) {
          timer.enabled = true;
          float percentComplete = 1.0f - (timeRemainingSeconds / timeUntilNextPhotoSeconds);
          timer.fillAmount = percentComplete;
          timeRemainingSeconds -= Time.deltaTime;
          yield return null;
        }
  
        MediaPlayerEventDispatcher.RaiseNextFile();
      }
    }
  }
}
