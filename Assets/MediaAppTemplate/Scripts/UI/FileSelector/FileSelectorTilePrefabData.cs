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
using System.Collections.Generic;
using System;

namespace Daydream.MediaAppTemplate {

  /// Serialized Data Model that defines the properties for a UI page that is used
  /// to display a category of levels that the player can select and load.
  [CreateAssetMenu(fileName = "FileSelectorTilePrefabData",
    menuName = "MediaApp/FileSelector/TilePrefabData",
    order = 1000)]
  public class FileSelectorTilePrefabData : ScriptableObject {
    [Serializable]
    public class ExtensionPrefab {
      public string extension;
      public GameObject prefab;
  
      [NonSerialized]
      public string prefabName;
    }

    [SerializeField]
    private GameObject directoryPrefab;
  
    [SerializeField]
    private GameObject defaultFilePrefab;
  
    [SerializeField]
    private ExtensionPrefab[] extentionsToPrefabs;
  
    private Dictionary<string, ExtensionPrefab> extensionPrefabsDict;
    private string directoryPrefabName;
    private string defaultFilePrefabName;

    public GameObject DirectoryPrefab {
      get {
        return directoryPrefab;
      }
    }

    public string DirectoryPrefabName {
      get {
        if (directoryPrefabName == null) {
          directoryPrefabName = directoryPrefab.name;
        }
  
        return directoryPrefabName;
      }
    }

    public GameObject DefaultFilePrefab {
      get {
        return defaultFilePrefab;
      }
    }

    private Dictionary<string, ExtensionPrefab> ExtensionPrefabsDict {
      get {
        if (extensionPrefabsDict != null) {
          return extensionPrefabsDict;
        }
  
        extensionPrefabsDict = new Dictionary<string, ExtensionPrefab>();
        for (int i = 0; i < extentionsToPrefabs.Length; i++) {
          ExtensionPrefab extensionPrefab = extentionsToPrefabs[i];
          extensionPrefabsDict[extensionPrefab.extension] = extensionPrefab;
        }
  
        return extensionPrefabsDict;
      }
    }

    public string DefaultFilePrefabName {
      get {
        if (defaultFilePrefabName == null) {
          defaultFilePrefabName = defaultFilePrefab.name;
        }
  
        return defaultFilePrefabName;
      }
    }

    public GameObject GetPrefabForExtension(string extension) {
      ExtensionPrefab result = null;
      if (ExtensionPrefabsDict.TryGetValue(extension, out result)) {
        return result.prefab;
      }
  
      return DefaultFilePrefab;
    }

    public string GetPrefabNameForExtension(string extension) {
      ExtensionPrefab result = null;
      if (ExtensionPrefabsDict.TryGetValue(extension, out result)) {
        if (result.prefabName == null) {
          result.prefabName = result.prefab.name;
        }
  
        return result.prefabName;
      }
  
      return DefaultFilePrefabName;
    }
  }
}
