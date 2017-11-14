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
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using System.Text;
using DaydreamElements.Common;

namespace Daydream.MediaAppTemplate {

  /// This script is a single UI tile that represents either a directory
  /// or a file in the user's file system.
  /// Used by _FileSelectorPageProvider_ and
  /// _FileSelectorPage_
  public class FileSelectorTile : MonoBehaviour {
    public event Action<DirectoryInfo> OnDirectorySelected;

    public delegate void FileSelectedDelegate(FileInfo file,int fileIndex);

    public event FileSelectedDelegate OnFileSelected;

    [SerializeField]
    private Text nameText;
  
    [SerializeField]
    private Image image;
  
    [SerializeField]
    private BaseTile baseTile;
  
    [SerializeField]
    private int maxNameLength = 20;
  
    private RectTransform imageTransform;
  
    private DirectoryInfo directory;
    private FileInfo file;
    private int fileIndex = -1;
    private Sprite originalSprite;
    private bool isInTransition = false;
    private bool hasRequestedThumbnail = false;
  
    private const string THUMBNAIL_CACHE_NAME = "FileThumbnails";
    private const int THUMBNAIL_CACHE_CAPACITY = 50;

    public string CachedPrefabName { get; private set; }

    public bool IsDirectory {
      get {
        return directory != null;
      }
    }

    public Image TileImage {
      get {
        return image;
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
        SetImageToThumbnail();
      }
    }

    public void Reset() {
      file = null;
      fileIndex = -1;
      directory = null;
      OnFileSelected = null;
      OnDirectorySelected = null;
      image.sprite = originalSprite;
      IsInTransition = false;
      hasRequestedThumbnail = false;
      CachedPrefabName = null;
      if (baseTile != null) {
        baseTile.Reset();
      }
    }

    public void SetToDirectory(DirectoryInfo directory) {
      file = null;
      fileIndex = -1;
  
      SetNameText(directory.Name);
      this.directory = directory;
    }

    public void SetToFile(StringBuilder displayName, FileInfo file, string prefabName, int fileIndex) {
      directory = null;
  
      SetNameText(displayName);
      this.file = file;
      this.fileIndex = fileIndex;
      CachedPrefabName = prefabName;
  
      SetImageToThumbnail();
    }

    void Awake() {
      Assert.IsNotNull(image);
      originalSprite = image.sprite;
      imageTransform = image.GetComponent<RectTransform>();
    }

    private void SetNameText(string name) {
      string truncatedString = StringHelpers.TruncateStringWithEllipsis(name, maxNameLength);
      nameText.text = truncatedString;
    }

    private void SetNameText(StringBuilder name) {
      StringHelpers.TruncateStringWithEllipsis(name, maxNameLength);
      nameText.text = name.ToString();
    }

    public void Select() {
      if (directory != null && OnDirectorySelected != null) {
        OnDirectorySelected(directory);
      } else if (file != null && OnFileSelected != null) {
        OnFileSelected(file, fileIndex);
      }
    }

    private void SetImageToThumbnail() {
      if (file == null || hasRequestedThumbnail) {
        return;
      }
  
      LRUCache<string, Sprite> thumbnailCache = null;
      thumbnailCache =
        CacheManager.Instance.GetCache<string, Sprite>(THUMBNAIL_CACHE_NAME, THUMBNAIL_CACHE_CAPACITY);
  
      if (thumbnailCache != null) {
        Sprite thumbnail = thumbnailCache.Get(file.FullName);
        if (thumbnail != null) {
          SetThumbnailFromSprite(thumbnail);
          hasRequestedThumbnail = true;
          return;
        }
      }
  
      if (IsInTransition) {
        // There was no cached thumbnail so try to load it asynchronously.
        MediaHelpers.GetThumbnail(file.FullName, OnThumbnailLoaded);
        hasRequestedThumbnail = true;
      }
    }

    private void OnThumbnailLoaded(string filePath, byte[] thumbnailBytes, bool cancelled) {
      if (file == null) {
        return;
      }
  
      if (file.FullName != filePath) {
        return;
      }
  
      if (thumbnailBytes == null) {
        if (cancelled) {
          MediaHelpers.GetThumbnail(file.FullName, OnThumbnailLoaded);
        }
        return;
      }
  
      Texture2D thumbnail = new Texture2D(2, 2);
      thumbnail.LoadImage(thumbnailBytes);
  
      Vector2 size = imageTransform.sizeDelta;
  
      float aspectRatio = size.x / size.y;
      float width = thumbnail.width;
      float height = thumbnail.height;
  
      height = width / aspectRatio;
      if (height > thumbnail.height) {
        height = thumbnail.height;
        width = height * aspectRatio;
      }
  
      float rectX = (thumbnail.width * 0.5f) - (width * 0.5f);
      float rectY = (thumbnail.height * 0.5f) - (height * 0.5f);
      Rect rect = new Rect(rectX, rectY, width, height);
      Sprite thumbnailSprite = Sprite.Create(thumbnail, rect, new Vector2(0.5f, 0.5f));
  
      LRUCache<string, Sprite> thumbnailCache = null;
      thumbnailCache =
        CacheManager.Instance.GetCache<string, Sprite>(THUMBNAIL_CACHE_NAME, THUMBNAIL_CACHE_CAPACITY);
      thumbnailCache.Set(file.FullName, thumbnailSprite);
  
      SetThumbnailFromSprite(thumbnailSprite);
    }

    private void SetThumbnailFromSprite(Sprite thumbnailSprite) {
      Assert.IsNotNull(image);
      image.sprite = thumbnailSprite;
      image.color = Color.white;
    }
  }
}
