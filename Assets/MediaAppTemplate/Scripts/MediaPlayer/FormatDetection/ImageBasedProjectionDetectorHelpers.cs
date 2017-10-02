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

  /// Helper functions for detecting the stereo projection format of a texture
  /// based on analyzing the texture.
  public static class ImageBasedProjectionDetectorHelpers {
    private const float SQUASHED_FRAME_THRESHOLD = 3.0f;

    /// Calculates the aspect ratio of a frame within a texture based on the textures stereo mode.
    public static float CalculateFrameAspectRatio(Texture texture, BaseMediaPlayer.StereoMode stereoMode) {
      // Avoid dividing by zero.
      if (texture.height == 0.0f) {
        return 0.0f;
      }

      float rawAspectRatio = (float)texture.width / (float)texture.height;
      return CalculateFrameAspectRatio(rawAspectRatio, stereoMode);
    }

    /// Calculates the aspect ratio of a frame within a texture based on the textures stereo mode.
    public static float CalculateFrameAspectRatio(float rawAspectRatio, BaseMediaPlayer.StereoMode stereoMode) {
      float result;

      switch (stereoMode) {
        case BaseMediaPlayer.StereoMode.LeftRight:
          result = rawAspectRatio / 2.0f;
          break;
        case BaseMediaPlayer.StereoMode.TopBottom:
          result = rawAspectRatio * 2.0f;
          break;
        default:
          result = rawAspectRatio;
          break;
      }

      // Some stereo textures pack the texture data of a frame by squashing the frame
      // and then expecting it to still be rendered at the original aspect ratio.
      // Make a best guess for when the frame is squashed...
      // TODO: Make this more robust.
      if (result > SQUASHED_FRAME_THRESHOLD) {
        result = rawAspectRatio;
      }

      return result;
    }
  }
}
