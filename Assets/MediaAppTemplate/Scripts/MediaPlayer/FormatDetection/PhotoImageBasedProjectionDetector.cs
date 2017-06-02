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

  /// Determines the stereo projection format of a photo by analyzing the texture.
  public class PhotoImageBasedProjectionDetector : ImageBasedProjectionDetector {
    public PhotoImageBasedProjectionDetector(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    protected override void DetectInternal() {
      BasePhotoPlayer photoPlayer = MediaPlayer as BasePhotoPlayer;
      if (photoPlayer == null) {
        Debug.LogError("Can't detect format, MediaPlayer must be a BasePhotoPlayer.");
        CompleteDetection(null);
        return;
      }
  
      AnalyzeImage(photoPlayer.PhotoTexture, OnAnalyzeImage);
    }

    private void OnAnalyzeImage(StereoProjectionFormat format) {
      CompleteDetection(format);
    }
  }
}
