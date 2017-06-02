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
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  [CustomEditor(typeof(PlaybackScrubber))]
  public class PlaybackScrubberEditor : SliderEditor {
    private SerializedProperty positionLabel;
    private SerializedProperty durationLabel;
    private SerializedProperty hoverHandle;
    private SerializedProperty hoverLabel;
    private SerializedProperty storyboardPrefab;

    protected override void OnEnable() {
      base.OnEnable();
      positionLabel = serializedObject.FindProperty(PlaybackScrubber.POSITION_LABEL_PROP_NAME);
      durationLabel = serializedObject.FindProperty(PlaybackScrubber.DURATION_LABEL_PROP_NAME);
      hoverHandle = serializedObject.FindProperty(PlaybackScrubber.HOVER_HANDLE_PROP_NAME);
      hoverLabel = serializedObject.FindProperty(PlaybackScrubber.HOVER_LABEL_PROP_NAME);
      storyboardPrefab = serializedObject.FindProperty(PlaybackScrubber.STORYBOARD_PROP_NAME);
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();
      EditorGUILayout.PropertyField(positionLabel);
      EditorGUILayout.PropertyField(durationLabel);
      EditorGUILayout.PropertyField(hoverHandle);
      EditorGUILayout.PropertyField(hoverLabel);
      EditorGUILayout.PropertyField(storyboardPrefab);
      serializedObject.ApplyModifiedProperties();

      base.OnInspectorGUI();
    }
  }
}
