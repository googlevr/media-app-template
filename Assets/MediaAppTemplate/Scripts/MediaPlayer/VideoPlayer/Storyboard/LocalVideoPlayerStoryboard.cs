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
using System.Collections;
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// Previews frames from a video file stored locally. Asynchronously retrieves frames
  /// along an interval of the video and stores them in an LRUCache. There may be a delay between
  /// setting PreviewPositionMilliseconds and seeing the preview rendered.
  [RequireComponent(typeof(Image))]
  public class LocalVideoPlayerStoryboard : BaseVideoPlayerStoryboard {
    [SerializeField]
    private int storyboardSteps = 50;

    private Image image;
    private RectTransform rectTransform;

    private PlayOptions playOptions;
    private long actualPosition = -1;
    private int storyboardFrame = -1;
    private long storyboardPosition = -1;
    private long videoDurationMilliseconds = -1;

    private const string PATH_PREFIX = "file://";
    private const int MAX_ACTIVE_REQUESTS = 1;
    private const int MAX_PENDING_REQUESTS = 10;

    private static LRUCache<int, Sprite> spriteCache = new LRUCache<int, Sprite>(50);
    private static HashSet<int> pendingFrames = new HashSet<int>();
    private static LinkedList<int> pendingFramesToRequest = new LinkedList<int>();
    private static int activeRequestsCount = 0;
    private static string previousPath;

    private const int FRAME_HEIGHT = 50;

    public long FrameIntervalMilliseconds {
      get {
        return videoDurationMilliseconds / storyboardSteps;
      }
    }

    public override long PreviewPositionMilliseconds {
      get {
        return actualPosition;
      }
      set {
        actualPosition = value;
        int newStoryboardFrame = (int)(actualPosition / FrameIntervalMilliseconds);
        if (newStoryboardFrame != storyboardFrame) {
          storyboardFrame = newStoryboardFrame;
          storyboardPosition = storyboardFrame * FrameIntervalMilliseconds;
          RequestFrame(storyboardPosition, storyboardFrame);
        }
      }
    }

    public override void Initialize(PlayOptions videoPlayOptions, long durationMilliseconds) {
      playOptions = videoPlayOptions;
      if (previousPath != playOptions.Path) {
        spriteCache.Clear();
        previousPath = playOptions.Path;
      }
      pendingFrames.Clear();
      pendingFramesToRequest.Clear();
      activeRequestsCount = 0;
      videoDurationMilliseconds = durationMilliseconds;
    }

    void Awake() {
      image = GetComponent<Image>();
      image.enabled = false;
      rectTransform = GetComponent<RectTransform>();
    }

    private void RequestFrame(long frameMilliseconds, int frame) {
      Sprite sprite;
      if (spriteCache.TryGetValue(frame, out sprite)) {
        if (sprite != null) {
          SetRendererToTexture(sprite);
        }
        return;
      }

      if (pendingFrames.Contains(frame)) {
        return;
      }

      pendingFrames.Add(frame);

      if (activeRequestsCount >= MAX_ACTIVE_REQUESTS) {
        pendingFramesToRequest.AddFirst(frame);
        while (pendingFramesToRequest.Count > MAX_PENDING_REQUESTS) {
          int pendingFrameToRemove = pendingFramesToRequest.Last.Value;
          pendingFramesToRequest.RemoveLast();
          pendingFrames.Remove(pendingFrameToRemove);
        }
      } else {
        LoadFrame(frameMilliseconds);
      }
    }

    private void OnFrameLoaded(byte[] frameData, long positionMilliseconds) {
      activeRequestsCount--;
      TryLoadPendingFrames();

      if (frameData == null) {
        return;
      }

      int frame = (int)(positionMilliseconds / FrameIntervalMilliseconds);
      Texture2D frameTexture = new Texture2D(2, 2);
      frameTexture.LoadImage(frameData);
      Rect rect = new Rect(0, 0, frameTexture.width, frameTexture.height);
      Sprite sprite = Sprite.Create(frameTexture, rect, new Vector2(0.5f, 0.5f));
      spriteCache.Set(frame, sprite);
      pendingFrames.Remove(frame);
      if (frame == storyboardFrame) {
        SetRendererToTexture(sprite);
      }
    }

    private void LoadFrame(long frameMilliseconds) {
      activeRequestsCount++;

      string path = playOptions.Path;
      if (path.StartsWith(PATH_PREFIX)) {
        path = path.Substring(PATH_PREFIX.Length, path.Length - PATH_PREFIX.Length);
      }
      // When the key exists but the value is null we are in the process of fetching this frame
      MediaHelpers.GetVideoFrame(path, frameMilliseconds, FRAME_HEIGHT, OnFrameLoaded);
    }

    private void TryLoadPendingFrames() {
      while (activeRequestsCount < MAX_ACTIVE_REQUESTS && pendingFramesToRequest.Count > 0) {
        int frame = pendingFramesToRequest.First.Value;
        pendingFramesToRequest.RemoveFirst();
        long frameMilliseconds = frame * FrameIntervalMilliseconds;
        LoadFrame(frameMilliseconds);
      }
    }

    private void SetRendererToTexture(Sprite sprite) {
      image.sprite = sprite;
      image.enabled = true;
      rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
    }
  }
}
