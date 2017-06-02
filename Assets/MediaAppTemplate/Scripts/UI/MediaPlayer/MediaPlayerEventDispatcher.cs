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

  /// Used to globally dispatch events that can occur when watching a video.
  public static class MediaPlayerEventDispatcher {
    public delegate void ExitMediaDelegate();

    public static event ExitMediaDelegate OnExitMedia;

    public static void RaiseExitMedia() {
      if (OnExitMedia != null) {
        OnExitMedia();
      }
    }

    public delegate void NextFileDelegate();

    public static event NextFileDelegate OnNextFile;

    public static void RaiseNextFile() {
      if (OnNextFile != null) {
        OnNextFile();
      }
    }

    public delegate void PreviousFileDelegate();

    public static event PreviousFileDelegate OnPreviousFile;

    public static void RaisePreviousFile() {
      if (OnPreviousFile != null) {
        OnPreviousFile();
      }
    }

    public delegate void SphericalScreenBeginDragDelegate();

    public static event SphericalScreenBeginDragDelegate OnSphericalScreenBeginDrag;

    public static void RaiseSphericalScreenBeginDrag() {
      if (OnSphericalScreenBeginDrag != null) {
        OnSphericalScreenBeginDrag();
      }
    }

    public delegate void SphericalScreenEndDragDelegate();

    public static event SphericalScreenEndDragDelegate OnSphericalScreenEndDrag;

    public static void RaiseSphericalScreenEndDrag() {
      if (OnSphericalScreenEndDrag != null) {
        OnSphericalScreenEndDrag();
      }
    }
  }
}
