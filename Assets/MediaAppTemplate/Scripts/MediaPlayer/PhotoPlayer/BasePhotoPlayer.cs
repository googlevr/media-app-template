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
using System.IO;

namespace Daydream.MediaAppTemplate {

  /// Base class for all Photo Players. Subclass to integrate Media App Template
  /// with a specific photo viewer implementation.
  public abstract class BasePhotoPlayer : BaseMediaPlayer {
    private string filePath;

    public override string FilePath {
      get {
        return filePath;
      }
    }

    public abstract Texture2D PhotoTexture { get; }

    public BasePhotoPlayer() : base() {
      detectorQueue.AddDetector(new PhotoImageBasedProjectionDetector(this));
    }

    public void ShowPhoto(FileInfo file) {
      DestroyScreen();

      filePath = file.FullName;
      ShowPhotoInternal(file);

      DetectStereoProjectionFormat();
    }

    public abstract void ShowPhotoInternal(FileInfo file);
  }
}