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

namespace Daydream.MediaAppTemplate {

  /// Simple implementation of an LRUCache. Trims the least recently used items
  /// when adding an item increases the item count higher than the capacity.
  public class LRUCache<K, V> {
    private class Item {
      public K key;
      public V value;
    }

    private int capacity;
    private Dictionary<K, LinkedListNode<Item>> map = new Dictionary<K, LinkedListNode<Item>>();
    private LinkedList<Item> linkedList = new LinkedList<Item>();

    public int Capacity {
      get {
        return capacity;
      }
      set {
        if (capacity == value) {
          return;
        }
  
        capacity = value;
        TrimCache();
      }
    }

    public LRUCache(int cacheCapacity) {
      capacity = cacheCapacity;
    }

    public bool Contains(K key) {
      return map.ContainsKey(key);
    }

    public V Get(K key) {
      LinkedListNode<Item> itemNode;
      if (map.TryGetValue(key, out itemNode)) {
        // We have accessed the key, so we need to move it to the front of the list.
        linkedList.Remove(itemNode);
        linkedList.AddLast(itemNode);
        return itemNode.Value.value;
      }
      return default(V);
    }

    public bool TryGetValue(K key, out V value) {
      LinkedListNode<Item> itemNode;
      if (map.TryGetValue(key, out itemNode)) {
        // We have accessed the key, so we need to move it to the front of the list.
        linkedList.Remove(itemNode);
        linkedList.AddLast(itemNode);
  
        value = itemNode.Value.value;
        return true;
      }
  
      value = default(V);
      return false;
    }

    public void Set(K key, V value) {
      // Remove key in case it is already set.
      Remove(key);
  
      Item item = new Item();
      item.key = key;
      item.value = value;
      LinkedListNode<Item> itemNode = linkedList.AddLast(item);
      map[key] = itemNode;
  
      TrimCache();
    }

    public void Remove(K key) {
      LinkedListNode<Item> itemNode;
      if (map.TryGetValue(key, out itemNode)) {
        linkedList.Remove(itemNode);
        map.Remove(key);
      }
    }

    public void Clear() {
      map.Clear();
      linkedList.Clear();
    }

    private void TrimCache() {
      while (map.Count > capacity) {
        LinkedListNode<Item> itemNode = linkedList.First;
        map.Remove(itemNode.Value.key);
        linkedList.RemoveFirst();
      }
    }
  }
}
