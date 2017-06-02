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
using System.IO;

namespace Daydream.MediaAppTemplate {

  /// This script is used to visualize the title of a video.
  public class PlaybackTitle : MonoBehaviour, IPlaybackControl {
    [SerializeField]
    private Text titleLabel;
  
    [SerializeField]
    private int maxTitleLength = 20;
  
    private BaseMediaPlayer mediaPlayer;

    public void Setup(BaseMediaPlayer player) {
      mediaPlayer = player;
      RefreshTitleLabel();
    }

    void Awake() {
      MediaPlayerEventDispatcher.OnNextFile += RefreshTitleLabel;
      MediaPlayerEventDispatcher.OnPreviousFile += RefreshTitleLabel;
    }

    void OnDestroy() {
      MediaPlayerEventDispatcher.OnNextFile -= RefreshTitleLabel;
      MediaPlayerEventDispatcher.OnPreviousFile -= RefreshTitleLabel;
    }

    private void RefreshTitleLabel() {
      if (titleLabel != null && mediaPlayer != null) {
        string title = Path.GetFileNameWithoutExtension(mediaPlayer.FilePath);
        string truncatedString = StringHelpers.TruncateStringWithEllipsis(title, maxTitleLength);
        titleLabel.text = truncatedString;
      }
    }
  }
}
