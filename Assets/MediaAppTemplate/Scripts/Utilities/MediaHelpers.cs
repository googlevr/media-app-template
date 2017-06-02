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
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// Utility functions for getting thumbnails and frames from a video asynchronously.
  /// Only implemented on Android currently.
  public static class MediaHelpers {
    public enum MediaType {
      Video,
      Photo,
      Invalid
    }

    public static readonly Dictionary<string, MediaType> EXTENSIONS_TO_MEDIA_TYPE = new Dictionary<string, MediaType>()
    {
      { ".mp4", MediaType.Video },
      { ".png", MediaType.Photo },
      { ".jpg", MediaType.Photo }
    };

    public static MediaType GetMediaType(FileInfo file) {
      return GetMediaType(file.Extension);
    }

    public static MediaType GetMediaType(string extension) {
      MediaType mediaType;
      if (EXTENSIONS_TO_MEDIA_TYPE.TryGetValue(extension, out mediaType)) {
        return mediaType;
      }

      return MediaType.Invalid;
    }

    public static void GetThumbnail(string path, Action<string, Byte[], bool> callback) {
      #if UNITY_ANDROID && !UNITY_EDITOR
      MediaHelpers.GetAndroidThumbnail(path, callback);
      #else
      if (callback != null) {
        callback(null, null, false);
      }
      #endif
    }

    public static void GetVideoFrame(string path, long positionMilliseconds, int frameHeight, Action<Byte[], long> callback) {
      #if UNITY_ANDROID && !UNITY_EDITOR
      MediaHelpers.GetAndroidVideoFrame(path, positionMilliseconds, frameHeight, callback);
      #else
      if (callback != null) {
        callback(null, 0);
      }
      #endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject mediaUtilities = null;

    private static AndroidJavaObject MediaUtilities {
      get {
        if (mediaUtilities == null) {
          mediaUtilities = new AndroidJavaObject("com.google.mediaapp.MediaUtilities");
          AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
          AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
          object[] args = new object[] { jo };
          mediaUtilities.Call("SetCurrentActivity", args);
        }

        return mediaUtilities;
      }
    }

    private class GetThumbnailCallback : AndroidJavaProxy {
      private string filePath;
      private Action<string, byte[], bool> callback;
      private bool isCallbackTargetUnityObject;

      public GetThumbnailCallback(string path, Action<string, byte[], bool> thumbnailCallback)
        : base("com.google.mediaapp.IGetThumbnailCallback") {
        callback = thumbnailCallback;
        isCallbackTargetUnityObject = (callback.Target as UnityEngine.Object) != null;
        filePath = path;
      }

      public void OnGetThumbnail(AndroidJavaObject thumbnailData) {
        byte[] data = null;
        bool cancelled = false;

        if (thumbnailData != null) {
          AndroidJavaObject dataObject = thumbnailData.Get<AndroidJavaObject>("Data");
          data = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(dataObject.GetRawObject());
          if (data.Length == 0) {
            data = null;
          }

          cancelled = thumbnailData.Get<bool>("Cancelled");
        }

        LifecycleManager.Instance.QueueOnUpdateAction(() => {
          bool isCallbackValid = callback != null;
          if (isCallbackTargetUnityObject) {
            isCallbackValid = isCallbackValid && (callback.Target as UnityEngine.Object) != null;
          }
          if (isCallbackValid) {
            callback(filePath, data, cancelled);
          }
        });
      }
    }

    private static void GetAndroidThumbnail(string path, Action<string, Byte[], bool> callback) {
      object[] args = new object[] { path, new GetThumbnailCallback(path, callback) };
      MediaUtilities.Call("GetThumbnail", args);
    }

    private class GetVideoFrameCallback : AndroidJavaProxy {
      private Action<byte[], long> callback;
      private bool isCallbackTargetUnityObject;

      public GetVideoFrameCallback(Action<byte[], long> frameCallback)
        : base("com.google.mediaapp.IGetVideoFrameCallback") {
        callback = frameCallback;
        isCallbackTargetUnityObject = (callback.Target as UnityEngine.Object) != null;
      }

      public void OnGetVideoFrame(AndroidJavaObject frameData) {
        byte[] data = null;
        long positionMilliseconds = 0;

        if (frameData != null) {
          AndroidJavaObject dataObject = frameData.Get<AndroidJavaObject>("Data");
          data = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(dataObject.GetRawObject());
          if (data.Length == 0) {
            data = null;
          }

          positionMilliseconds = frameData.Get<long>("PositionMilliseconds");
        }

        LifecycleManager.Instance.QueueOnUpdateAction(() => {
          bool isCallbackValid = callback != null;
          if (isCallbackTargetUnityObject) {
            isCallbackValid = isCallbackValid && (callback.Target as UnityEngine.Object) != null;
          }
          if (isCallbackValid) {
            callback(data, positionMilliseconds);
          }
        });
      }
    }

    private static void GetAndroidVideoFrame(string path, long positionMilliseconds, int frameHeight, Action<Byte[], long> callback) {
      object[] args = new object[] { path, positionMilliseconds, frameHeight, new GetVideoFrameCallback(callback) };
      MediaUtilities.Call("GetFrameForVideo", args);
    }
#endif
  }
}
