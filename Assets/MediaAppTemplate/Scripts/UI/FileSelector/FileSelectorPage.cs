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
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaydreamElements.Common;

namespace Daydream.MediaAppTemplate {

  /// This script represents a single page in the file selector.
  /// Each tile in the page represents either a directory or a file.
  [RequireComponent(typeof(TiledPage))]
  public class FileSelectorPage : MonoBehaviour {
    [SerializeField]
    private FileSelectorTilePrefabData prefabData;
  
    [SerializeField]
    private LayoutGroup layoutGroup;
  
    [SerializeField]
    private int maxTileCount;
  
    private List<FileSelectorTile> tiles = new List<FileSelectorTile>();
    private TiledPage tiledPage;
    private Coroutine refreshTiledPageCoroutine;
    private bool isInTransition = false;

    public int MaxTileCount {
      get {
        return maxTileCount;
      }
    }

    public bool IsInTransition {
      get {
        return isInTransition;
      }
      set {
        if (isInTransition == value) {
          return;
        }
  
        isInTransition = value;
  
        for (int i = 0; i < tiles.Count; i++) {
          FileSelectorTile tile = tiles[i];
          tile.IsInTransition = isInTransition;
        }
      }
    }

    void Awake() {
      tiledPage = GetComponent<TiledPage>();
      Assert.IsNotNull(tiledPage);
    }

    public void Reset() {
      for (int i = 0; i < tiles.Count; i++) {
        FileSelectorTile tile = tiles[i];
  
        GameObjectPool pool;
        if (tile.IsDirectory) {
          pool = ObjectPoolManager.Instance.GetPool<GameObjectPool>(prefabData.DirectoryPrefabName);
        } else {
          pool = ObjectPoolManager.Instance.GetPool<GameObjectPool>(tile.CachedPrefabName);
        }
  
        tile.Reset();
  
        if (pool != null) {
          pool.Return(tile.gameObject);
        } else {
          GameObject.Destroy(tile.gameObject);
        }
      }
      tiles.Clear();
      tiledPage.Tiles = null;
      IsInTransition = false;
    }

    public FileSelectorTile AddDirectoryTile(DirectoryInfo directory) {
      FileSelectorTile tile = AddTile(prefabData.DirectoryPrefab, prefabData.DirectoryPrefabName);
      tile.SetToDirectory(directory);
      return tile;
    }

    /// fileIndex is NOT the index on the page. It is the index in the directory.
    public FileSelectorTile AddFileTile(FileInfo file, int fileIndex) {
      StringBuilder displayName, extension;
      StringHelpers.GetNameWithoutExtension(file, out displayName, out extension);
      string extensionString = extension.ToString();
      GameObject prefab = prefabData.GetPrefabForExtension(extensionString);
      string prefabName = prefabData.GetPrefabNameForExtension(extensionString);
      FileSelectorTile tile = AddTile(prefab, prefabName);
      tile.SetToFile(displayName, file, prefabName, fileIndex);
      return tile;
    }

    private FileSelectorTile AddTile(GameObject prefab, string poolName) {
      GameObjectPool pool = GetPool(prefab, poolName);
      GameObject tileObject = pool.Borrow();
      tileObject.transform.SetParent(layoutGroup.transform, false);
      tileObject.transform.SetSiblingIndex(tiles.Count);
  
      FileSelectorTile tile = tileObject.GetComponent<FileSelectorTile>();
      Assert.IsNotNull(tile);
      tiles.Add(tile);
      MarkTilesDirty();
      return tile;
    }

    private void MarkTilesDirty() {
      if (refreshTiledPageCoroutine != null) {
        return;
      }
  
      refreshTiledPageCoroutine = StartCoroutine(RefreshTiledPageDelayed());
    }

    private IEnumerator RefreshTiledPageDelayed() {
      yield return null;
      Transform[] tileTransforms = tiles.Select(tile => tile.TileImage.transform).ToArray();
      tiledPage.Tiles = tileTransforms;
      refreshTiledPageCoroutine = null;
    }

    private GameObjectPool GetPool(GameObject prefab, string poolName) {
      ObjectPoolManager poolManager = ObjectPoolManager.Instance;
      Assert.IsNotNull(poolManager);
  
      GameObjectPool pool =
        poolManager.GetPool<GameObjectPool>(poolName);
  
      if (pool != null) {
        return pool;
      }
  
      pool = new GameObjectPool(prefab, MaxTileCount * 2);
      poolManager.AddPool(poolName, pool);
  
      return pool;
    }
  }
}
