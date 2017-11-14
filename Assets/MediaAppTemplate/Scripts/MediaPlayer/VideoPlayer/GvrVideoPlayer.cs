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

namespace Daydream.MediaAppTemplate {

  /// BaseVideoPlayer implementation for the GvrVideoPlugin.
  [RequireComponent(typeof(GvrVideoPlayerTexture))]
  public class GvrVideoPlayer : BaseVideoPlayer {
    private GvrVideoPlayerTexture videoPlayer;
    private Coroutine briefUnpauseCoroutine;
    private Coroutine queuedPlayCoroutine;

    public override bool IsVideoLoaded {
      get {
        if (videoPlayer == null) {
          return false;
        }

        return videoPlayer.VideoReady && queuedPlayCoroutine == null;
      }
    }

    public override bool IsPlaying {
      get {
        if (videoPlayer == null) {
          return false;
        }

        return videoPlayer.VideoReady && !Paused;
      }
    }

    public override bool Paused {
      get {
        if (videoPlayer == null) {
          return false;
        }

        return videoPlayer.IsPaused || briefUnpauseCoroutine != null;
      }
      set {
        if (videoPlayer == null) {
          return;
        }

        if (value == videoPlayer.IsPaused) {
          return;
        }

        if (value) {
          videoPlayer.Pause();
        } else {
          StopBriefUnpause();
          videoPlayer.Play();
        }
      }
    }

    public override long CurrentPositionMilliseconds {
      get {
        if (videoPlayer == null) {
          return 0;
        }

        return videoPlayer.CurrentPosition;
      }
      set {
        if (videoPlayer == null) {
          return;
        }

        if (videoPlayer.CurrentPosition == value) {
          return;
        }

        videoPlayer.CurrentPosition = value;

        if (videoPlayer.IsPaused) {
          QueueBriefUnpauseIfNeeded();
        }
      }
    }

    public override long DurationMilliseconds {
      get {
        if (videoPlayer == null) {
          return 0;
        }

        return videoPlayer.VideoDuration;
      }
    }

    public override bool HasSphericalMetadata {
      get {
        if (videoPlayer == null) {
          return false;
        }

        return videoPlayer.CurrentStereoMode != GvrVideoPlayerTexture.StereoMode.NoValue;
      }
    }

    public override StereoMode MetadataStereoMode {
      get {
        if (videoPlayer == null) {
          return StereoMode.Unknown;
        }

        switch (videoPlayer.CurrentStereoMode) {
          case GvrVideoPlayerTexture.StereoMode.LeftRight:
            return StereoMode.LeftRight;
          case GvrVideoPlayerTexture.StereoMode.TopBottom:
            return StereoMode.TopBottom;
          case GvrVideoPlayerTexture.StereoMode.Mono:
            return StereoMode.Mono;
          default:
            return StereoMode.Unknown;
        }
      }
    }

    public override bool HasProjectionMetadata {
      get {
        if (videoPlayer == null) {
          return false;
        }

        return videoPlayer.HasProjection;
      }
    }

    public override Texture CurrentFrameTexture {
      get {
        if (videoPlayer == null) {
          return null;
        }

        if (queuedPlayCoroutine != null) {
          return null;
        }

        return videoPlayer.CurrentFrameTexture;
      }
    }

    public override float RawAspectRatio {
      get {
        if (videoPlayer == null) {
          return 0.0f;
        }

        return videoPlayer.AspectRatio;
      }
    }

    protected override void PlayInternal(PlayOptions options) {
      StopBriefUnpause();
      StopQueuedPlay();
      QueuePlay(options);
    }

    public override void Stop() {
      videoPlayer.CleanupVideo();
    }

    protected override void Awake() {
      videoPlayer = GetComponent<GvrVideoPlayerTexture>();
      base.Awake();
    }

    private void QueueBriefUnpauseIfNeeded() {
      if (briefUnpauseCoroutine != null) {
        return;
      }

      briefUnpauseCoroutine = StartCoroutine(BriefUnpause());
    }

    private void StopBriefUnpause() {
      if (briefUnpauseCoroutine != null) {
        StopCoroutine(briefUnpauseCoroutine);
        briefUnpauseCoroutine = null;
      }
    }

    private IEnumerator BriefUnpause() {
      do {
        yield return new WaitForEndOfFrame();
      } while (!videoPlayer.VideoReady);

      videoPlayer.Play();

      do {
        yield return new WaitForEndOfFrame();
      } while (!videoPlayer.VideoReady);

      videoPlayer.Pause();
      briefUnpauseCoroutine = null;
    }

    private void QueuePlay(PlayOptions options) {
      if (queuedPlayCoroutine != null) {
        return;
      }

      queuedPlayCoroutine = StartCoroutine(QueuePlayCoroutine(options));
    }

    private void StopQueuedPlay() {
      if (queuedPlayCoroutine != null) {
        StopCoroutine(queuedPlayCoroutine);
        queuedPlayCoroutine = null;
      }
    }

    private IEnumerator QueuePlayCoroutine(PlayOptions options) {
      bool needsRestart = !string.IsNullOrEmpty(videoPlayer.videoURL);
      if (needsRestart) {
        Destroy(videoPlayer);
        videoPlayer = gameObject.AddComponent<GvrVideoPlayerTexture>();
        videoPlayer.Screen = MediaScreen;
      }

      videoPlayer.videoType = (GvrVideoPlayerTexture.VideoType)options.Type;
      videoPlayer.videoURL = options.Path;

      yield return new WaitForEndOfFrame();

      videoPlayer.Init();

      while (!videoPlayer.VideoReady) {
        yield return new WaitForEndOfFrame();
      }

      videoPlayer.Play();

      queuedPlayCoroutine = null;
      yield return null;
    }

    protected override void SetupScreenInternal() {
      if (videoPlayer == null) {
        return;
      }

      videoPlayer.Screen = MediaScreen;
    }
  }
}
