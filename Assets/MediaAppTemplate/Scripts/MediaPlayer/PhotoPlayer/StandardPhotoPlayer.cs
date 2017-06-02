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
using System.Collections;
using System.IO;

namespace Daydream.MediaAppTemplate {

  /// Standard imlementation for a photo viewer with the media app template.
  public class StandardPhotoPlayer : BasePhotoPlayer {
    private Texture2D photoTexture = null;

    public override Texture2D PhotoTexture {
      get {
        return photoTexture;
      }
    }

    public override void ShowPhotoInternal(FileInfo file) {
      if (!file.Exists) {
        return;
      }

      DisposePhotoTexture();

      byte[] textureBytes = File.ReadAllBytes(file.FullName);
      photoTexture = new Texture2D(2, 2);
      photoTexture.LoadImage(textureBytes);
    }

    protected override void OnDestroy() {
      base.OnDestroy();
      DisposePhotoTexture();
    }

    private void DisposePhotoTexture() {
      if (photoTexture != null) {
        Destroy(photoTexture);
      }
    }

    protected override void SetupScreenInternal() {
      if (photoTexture == null) {
        return;
      }

      MediaScreen.material.mainTexture = photoTexture;
    }
  }
}