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
using UnityEngine.EventSystems;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// This script is a button used to dipatch video player events.
  public class PlaybackDispatchEvent : MonoBehaviour, IPointerClickHandler {
    public enum Event {
      ExitMedia,
      NextFile,
      PreviousFile
    }

    public Event eventType = Event.ExitMedia;

    public void OnPointerClick(PointerEventData eventData) {
      switch (eventType) {
        case Event.ExitMedia:
          MediaPlayerEventDispatcher.RaiseExitMedia();
          break;
        case Event.NextFile:
          MediaPlayerEventDispatcher.RaiseNextFile();
          break;
        case Event.PreviousFile:
          MediaPlayerEventDispatcher.RaisePreviousFile();
          break;
        default:
          Debug.LogError("Unsupported Event: " + eventType);
          break;
      }
    }
  }
}
