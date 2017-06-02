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
using System.Collections;
using System;

namespace Daydream.MediaAppTemplate {

  /// Data model representing the format of a media file.
  /// It is serializable so that the format can be persisted between sessions.
  [Serializable]
  public class StereoProjectionFormat {
    public BaseMediaPlayer.StereoMode stereoMode;
    public BaseMediaPlayer.ProjectionMode projectionMode;
    public float frameAspectRatio;
  }

  /// Subclass this to implement a methodology for detecting
  /// the stereo projection format of a media file.
  public abstract class BaseStereoProjectionDetector {
    private Action<StereoProjectionFormat> resultCallback;

    public bool IsDetecting {
      get {
        return resultCallback != null;
      }
    }

    protected BaseMediaPlayer MediaPlayer { get; private set; }

    public BaseStereoProjectionDetector(BaseMediaPlayer mediaPlayer) {
      MediaPlayer = mediaPlayer;
    }

    /// Detect the stereo projection format of the media player.
    public void Detect(Action<StereoProjectionFormat> callback) {
      ResetDetection();
      resultCallback = callback;
      DetectInternal();
    }

    /// Called before detection occurs.
    /// Can only be running one detection at a time, this
    /// function should cleanup any state related to an ongoing detection.
    public virtual void ResetDetection() {
      resultCallback = null;
    }

    /// Implement detection methodology.
    protected abstract void DetectInternal();

    /// All subclasses of BaseStereoProjectionDetector MUST call this function
    /// when detection is finished. If unable to detect a format, pass null
    /// into this function.
    protected virtual void CompleteDetection(StereoProjectionFormat format) {
      if (resultCallback != null) {
        resultCallback(format);
      }
      ResetDetection();
    }
  }
}
