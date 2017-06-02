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

  /// Attempts to detect the stereo projection format of a video
  /// based on the metadata within the video.
  public class VideoMetadataProjectionDetector : BaseStereoProjectionDetector {
    public VideoMetadataProjectionDetector(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    protected override void DetectInternal() {
      BaseVideoPlayer videoPlayer = MediaPlayer as BaseVideoPlayer;
      if (videoPlayer == null) {
        CompleteDetection(null);
        return;
      }
  
      videoPlayer.StartCoroutine(RunDetection(videoPlayer));
    }

    private IEnumerator RunDetection(BaseVideoPlayer videoPlayer) {
      while (!videoPlayer.IsVideoLoaded) {
        yield return null;
      }
  
      // It isn't guaranteed that metadata is available before these conditions are met
      // in the Gvr Video Plugin.
      while (videoPlayer.CurrentPositionMilliseconds == 0 || !videoPlayer.IsPlaying) {
        yield return null;
      }
  
      if (!videoPlayer.HasSphericalMetadata) {
        CompleteDetection(null);
      } else {
        StereoProjectionFormat format = new StereoProjectionFormat();
        format.stereoMode = videoPlayer.MetadataStereoMode;
  
        // TODO: Proper parsing of projection box in GvrVideoPlugin.
        if (videoPlayer.HasProjectionMetadata) {
          format.projectionMode = BaseMediaPlayer.ProjectionMode.Projection360;
        }
  
        CompleteDetection(format);
      }
    }
  }
}
