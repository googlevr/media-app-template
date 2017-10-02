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
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// Condition that determines if the controller is currently idle.
  /// The controller is considered idle if it hasn't moved by more than the
  /// threshold maxRotationChangeDegrees in TimeBeforeIdleSeconds.
  public class ControllerIdleCondition : BaseCondition {
    public const string IDENTIFIER = "ControllerIdle";
  
    [Tooltip("Seconds that must elapse before the controller becomes idle.")]
    public float TimeBeforeIdleSeconds = 1.5f;
  
    [SerializeField]
    [Tooltip("The degrees that the controller can rotate while still being considered idle.")]
    private float maxRotationChangeDegrees = 0.5f;
  
    [SerializeField]
    private ConditionsChecker resetIdlingCondition;
  
    private bool wasResetIdlingConditionMet = false;

    public override string ConditionIdentifier {
      get {
        return IDENTIFIER;
      }
    }

    public override bool CheckCondition() {
      return idleTimestampSeconds != null && Time.time >= idleTimestampSeconds.Value;
    }

    public float MaxRotationChangeDegrees {
      get {
        return maxRotationChangeDegrees;
      }
      set {
        maxRotationChangeDegrees = value;
        maxRotationChangeDot = Mathf.Cos(Mathf.Deg2Rad * maxRotationChangeDegrees);
      }
    }

    private Quaternion? lastRecordedRotation;
    private float maxRotationChangeDot;
    private float? idleTimestampSeconds = null;

    void Awake() {
      // This calculates the initial maxRotationChangeDot.
      MaxRotationChangeDegrees = MaxRotationChangeDegrees;
    }

    void OnDisable() {
      lastRecordedRotation = null;
      idleTimestampSeconds = null;
    }

    void Update() {
      bool resetIdlingConditionMet = resetIdlingCondition.Evaluate();
      if (!wasResetIdlingConditionMet && resetIdlingConditionMet) {
        idleTimestampSeconds = null;
      }
      wasResetIdlingConditionMet = resetIdlingConditionMet;
  
      if (lastRecordedRotation != null) {
        float dot = Quaternion.Dot(lastRecordedRotation.Value, GvrControllerInput.Orientation);
        if (dot < maxRotationChangeDot) {
          idleTimestampSeconds = null;
        } else if (idleTimestampSeconds == null) {
          idleTimestampSeconds = Time.time + TimeBeforeIdleSeconds;
        }
      }
  
      lastRecordedRotation = GvrControllerInput.Orientation;
    }
  }
}
