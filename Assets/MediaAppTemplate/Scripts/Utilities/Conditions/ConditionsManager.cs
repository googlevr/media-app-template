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

  /// This class is a singleton that is used to register conditions by an identifier that can
  /// then be globally accessed. Conditions can be checked in the editor via a _ConditionChecker_.
  public class ConditionsManager : MonoBehaviour {
    private static ConditionsManager instance;

    public static ConditionsManager Instance {
      get {
        return instance;
      }
    }

    private Dictionary<string, HashSet<BaseCondition>> registeredConditions;

    public bool IsConditionTrue(string conditionIdentifier) {
      HashSet<BaseCondition> conditionsSet;
      if (!registeredConditions.TryGetValue(conditionIdentifier, out conditionsSet)) {
        return false;
      }
  
      foreach (BaseCondition condition in conditionsSet) {
        if (condition.CheckCondition()) {
          return true;
        }
      }
  
      return false;
    }

    public HashSet<BaseCondition> GetConditions(string conditionIdentifier) {
      HashSet<BaseCondition> conditionsSet;
      registeredConditions.TryGetValue(conditionIdentifier, out conditionsSet);
      return conditionsSet;
    }

    public void RegisterCondition(BaseCondition condition) {
      HashSet<BaseCondition> conditionsSet;
      if (!registeredConditions.TryGetValue(condition.ConditionIdentifier, out conditionsSet)) {
        conditionsSet = new HashSet<BaseCondition>();
        registeredConditions[condition.ConditionIdentifier] = conditionsSet;
      }
  
      if (!conditionsSet.Contains(condition)) {
        conditionsSet.Add(condition);
      }
    }

    public void UnregisterCondition(BaseCondition condition) {
      HashSet<BaseCondition> conditionsSet;
      if (!registeredConditions.TryGetValue(condition.ConditionIdentifier, out conditionsSet)) {
        return;
      }
  
      if (conditionsSet.Contains(condition)) {
        conditionsSet.Remove(condition);
      }
    }

    void Awake() {
      if (instance != null) {
        Debug.LogError("Cannot have multiple instances of Conditions Manager.");
      }
      instance = this;
      registeredConditions = new Dictionary<string, HashSet<BaseCondition>>();
    }
  }
}
