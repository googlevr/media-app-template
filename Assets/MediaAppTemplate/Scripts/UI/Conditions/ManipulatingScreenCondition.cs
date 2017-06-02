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

  /// Condition that determines if the video screen is currently being manipulated.
  /// The screen is being manipulated if it is being dragged OR if it is being zoomed.
  public class ManipulatingScreenCondition : BaseCondition {
    public const string IDENTIFIER = "ManipulatingScreen";
  
    private MediaScreenController mediaScreenController;
    private SphericalScreenController sphericalScreenController;

    public override string ConditionIdentifier {
      get {
        return IDENTIFIER;
      }
    }

    public override bool CheckCondition() {
      return IsManipulatingMediaScreen() || IsManipulatingSphericalScreen();
    }

    void Awake() {
      mediaScreenController = GetComponent<MediaScreenController>();
      sphericalScreenController = GetComponent<SphericalScreenController>();
    }

    private bool IsManipulatingMediaScreen() {
      if (mediaScreenController == null) {
        return false;
      }
  
      return mediaScreenController.IsManipulatingScreen;
    }

    private bool IsManipulatingSphericalScreen() {
      if (sphericalScreenController == null) {
        return false;
      }
  
      return sphericalScreenController.IsDragging;
    }
  }
}
