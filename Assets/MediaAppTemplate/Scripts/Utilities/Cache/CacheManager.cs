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
using System.Linq;
using System;

namespace Daydream.MediaAppTemplate {

  /// Provides named caches of any types.
  /// Caches will continue to exist as long as the CacheManager exists.
  public class CacheManager : MonoBehaviour {
    private static CacheManager instance;

    public static CacheManager Instance {
      get {
        return instance;
      }
    }

    private Hashtable table = new Hashtable();

    void Awake() {
      if (instance != null) {
        Debug.LogError("Cannot have multiple instances of CacheManager.");
      }
      instance = this;
    }

    public LRUCache<K, V> GetCache<K, V>(string cacheName, int capacity) {
      LRUCache<K, V> cache = null;
  
      if (table.Contains(cacheName)) {
        object cacheObj = table[cacheName];
        cache = cacheObj as LRUCache<K, V>;
        if (cache != null) {
          cache.Capacity = capacity;
        } else {
          Debug.LogError("Cache named " + cacheName + " already exists with different type: " + cacheObj);
        }
      } else {
        cache = new LRUCache<K, V>(capacity);
        table.Add(cacheName, cache);
      }
  
      return cache;
    }

    public void RemoveCache(string cacheName) {
      if (!table.Contains(cacheName)) {
        return;
      }
  
      table.Remove(cacheName);
    }

    public void ClearAllCaches() {
      table.Clear();
    }
  }
}
