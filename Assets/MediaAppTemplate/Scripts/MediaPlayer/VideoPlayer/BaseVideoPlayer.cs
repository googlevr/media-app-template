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
using System;
using System.Collections;

namespace Daydream.MediaAppTemplate {
  
  /// Base class for all Video Players. Subclass to integrate Media App Template
  /// with a specific Video Player implementation.
  public abstract class BaseVideoPlayer : BaseMediaPlayer {
    public enum VideoType {
      Dash = 0,
      HLS = 2,
      Other = 3
    };

    public event Action OnPlay;

    [SerializeField]
    private PlayOptions startingVideo;

    public override string FilePath {
      get {
        return CurrentVideo.Path;
      }
    }

    public bool IsVideoLoadedAndDisplayed {
      get {
        return IsVideoLoaded && MediaScreen != null;
      }
    }

    public abstract bool IsVideoLoaded { get; }
    public abstract bool IsPlaying { get; }
    public abstract bool Paused { get; set; }
    public abstract bool HasSphericalMetadata { get; }
    public abstract StereoMode MetadataStereoMode { get; }
    public abstract bool HasProjectionMetadata { get; }
    public abstract long CurrentPositionMilliseconds { get; set; }
    public abstract long DurationMilliseconds { get; }
    public abstract void Stop();
    public abstract Texture CurrentFrameTexture { get; }
    protected abstract void PlayInternal(PlayOptions options);

    public PlayOptions CurrentVideo { get; private set; }

    public BaseVideoPlayer() : base() {
      detectorQueue.AddDetector(new VideoMetadataProjectionDetector(this));
      detectorQueue.AddDetector(new LocalVideoImageBasedProjectionDetector(this));
    }

    public void Play(PlayOptions options) {
      DestroyScreen();

      PlayInternal(options);
      CurrentVideo = options;

      if (OnPlay != null) {
        OnPlay();
      }

      DetectStereoProjectionFormat();
    }

    public void Restart() {
      PlayInternal(CurrentVideo);
    }

    protected override void Awake() {
      base.Awake();
      if (!string.IsNullOrEmpty(startingVideo.Path)) {
        Play(startingVideo);
      }
    }
  }
}
