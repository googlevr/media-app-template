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
using System;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// Determines the stereo projection format of a video by analyzing a frame
  /// one quarter of the way through the video.
  public class LocalVideoImageBasedProjectionDetector : ImageBasedProjectionDetector {
    private Coroutine analyzeVideoFrameCoroutine;
    private Texture2D frameTexture;

    private const string PATH_PREFIX = "file://";

    public LocalVideoImageBasedProjectionDetector(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    public override void ResetDetection() {
      base.ResetDetection();
      if (analyzeVideoFrameCoroutine != null) {
        MediaPlayer.StopCoroutine(analyzeVideoFrameCoroutine);
        analyzeVideoFrameCoroutine = null;
      }

      if (frameTexture != null) {
        GameObject.Destroy(frameTexture);
      }
    }

    protected override void DetectInternal() {
      BaseVideoPlayer videoPlayer = MediaPlayer as BaseVideoPlayer;
      if (videoPlayer == null) {
        Debug.LogError("Can't detect format, MediaPlayer must be a BaseVideoPlayer.");
        CompleteDetection(null);
        return;
      }

      analyzeVideoFrameCoroutine = MediaPlayer.StartCoroutine(AnalyzeVideoFrame(videoPlayer));
    }

    protected override void PostAnalyzeImage() {
      base.PostAnalyzeImage();

      BaseVideoPlayer videoPlayer = MediaPlayer as BaseVideoPlayer;
      // Should never reach this point if the media player isn't a video player.
      Assert.IsNotNull(videoPlayer);

      videoPlayer.Paused = false;
    }

    private IEnumerator AnalyzeVideoFrame(BaseVideoPlayer videoPlayer) {
      while (!videoPlayer.IsVideoLoaded) {
        yield return null;
      }

      // It isn't guaranteed that metadata is available before these conditions are met
      // in the Gvr Video Plugin.
      while (videoPlayer.CurrentPositionMilliseconds == 0 || !videoPlayer.IsPlaying) {
        yield return null;
      }

      videoPlayer.Paused = true;

      LoadFrameAndAnalyze(videoPlayer);
    }

    private void LoadFrameAndAnalyze(BaseVideoPlayer videoPlayer) {
      string path = videoPlayer.FilePath;
      if (path.StartsWith(PATH_PREFIX)) {
        path = path.Substring(PATH_PREFIX.Length, path.Length - PATH_PREFIX.Length);
      }

      // Seek to an arbitrary frame in hopes that it has good image data to analyze.
      // and isn't just a black fade transition or something like that.
      long framePositionMilliseconds = videoPlayer.DurationMilliseconds / 4;

      MediaHelpers.GetVideoFrame(path, framePositionMilliseconds, -1, OnFrameLoaded);
    }

    private void OnFrameLoaded(byte[] frameData, long positionMilliseconds) {
      if (frameData == null) {
        Debug.LogError("Unable to load frame.");
        CompleteDetection(null);
        return;
      }

      frameTexture = new Texture2D(2, 2);
      frameTexture.LoadImage(frameData);

      AnalyzeImage(frameTexture, CompleteDetection);
    }
  }
}
