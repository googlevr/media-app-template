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
using System;
using System.Collections;

namespace Daydream.MediaAppTemplate {

  /// Base class for all Media Players. A Media Player can
  /// be used to play any type of media (i.e. video/image/audio).
  public abstract class BaseMediaPlayer : MonoBehaviour {
    public enum StereoMode {
      Mono,
      LeftRight,
      TopBottom,
      Unknown
    }

    public enum ProjectionMode {
      Flat,
      Projection360,
      Projection180
    }

    public event Action<StereoMode> OnStereoModeChanged;
    public event Action<ProjectionMode> OnProjectionModeChanged;
    public event Action<Renderer> OnMediaScreenChanged;

    [SerializeField]
    private GameObject flatPrefab;

    [SerializeField]
    private GameObject Projection180Prefab;

    [SerializeField]
    private GameObject Projection360Prefab;

    protected ProjectionDetectorQueue detectorQueue;

    private StereoMode stereoMode = StereoMode.Mono;
    private ProjectionMode projectionMode = ProjectionMode.Flat;
    private float currentAspectRatio;

    private SavedStereoProjectionDetector savedStereoProjectionDetector;

    private Renderer mediaScreen;

    public abstract string FilePath { get; }

    public virtual Vector3 LocalCenter {
      get {
        if (MediaScreen == null) {
          return Vector3.zero;
        }

        return MediaScreen.transform.localPosition;
      }
    }

    public virtual Vector3 Center {
      get {
        if (MediaScreen == null) {
          return Camera.main.transform.position;
        }

        return MediaScreen.transform.position;
      }
    }

    public virtual Vector3 LocalSize {
      get {
        if (MediaScreen == null) {
          return Vector3.one;
        }

        return MediaScreen.transform.localScale;
      }
    }

    public virtual StereoMode CurrentStereoMode {
      get {
        return stereoMode;
      }
      set {
        if (stereoMode == value) {
          return;
        }

        stereoMode = value;
        SetShaderParameters();

        if (OnStereoModeChanged != null) {
          OnStereoModeChanged(stereoMode);
        }
      }
    }

    public virtual ProjectionMode CurrentProjectionMode {
      get {
        return projectionMode;
      }
      set {
        if (projectionMode == value) {
          return;
        }

        projectionMode = value;
        DestroyScreen();
        InitScreen();

        if (OnProjectionModeChanged != null) {
          OnProjectionModeChanged(projectionMode);
        }
      }
    }

    public float CurrentAspectRatio {
      get {
        return currentAspectRatio;
      }
      set {
        currentAspectRatio = value;

        if (MediaScreen == null) {
          return;
        }

        MediaScreenController screenController =
          MediaScreen.GetComponent<MediaScreenController>();

        if (screenController == null) {
          return;
        }

        screenController.AspectRatio = currentAspectRatio;
      }
    }

    /// The raw aspect ratio of the texture for this media.
    /// CurrentAspectRatio may be different due to the current StereoMode.
    public virtual float RawAspectRatio {
      get {
        if (mediaScreen == null) {
          return 0.0f;
        }

        Texture texture = mediaScreen.sharedMaterial.mainTexture;
        if (texture == null) {
          return 0.0f;
        }

        return (float)texture.width / (float)texture.height;
      }
    }

    public Renderer MediaScreen {
      get {
        return mediaScreen;
      }
      set {
        if (value == mediaScreen) {
          return;
        }

        mediaScreen = value;

        if (OnMediaScreenChanged != null) {
          OnMediaScreenChanged(mediaScreen);
        }
      }
    }

    public BaseMediaPlayer() {
      detectorQueue = new ProjectionDetectorQueue(this);
      savedStereoProjectionDetector = new SavedStereoProjectionDetector(this);
      detectorQueue.AddDetector(savedStereoProjectionDetector);
    }

    protected abstract void SetupScreenInternal();

    public void DetectStereoProjectionFormat() {
      detectorQueue.Detect(OnFormatDetected);
    }

    private void OnFormatDetected(StereoProjectionFormat format) {
      // If there was no format detected,
      // Then use the default format.
      if (format == null) {
        format = new StereoProjectionFormat();
      }

      CurrentAspectRatio = format.frameAspectRatio;
      CurrentStereoMode = format.stereoMode;

      bool changedProjection = false;
      if (CurrentProjectionMode != format.projectionMode) {
        CurrentProjectionMode = format.projectionMode;
        changedProjection = true;
      }

      // The screen is automatically re-initialized when the projection is detected,
      // make sure we don't init it twice.
      if (!changedProjection) {
        InitScreen();
      }

      SaveCurrentFormat();
    }

    public void SaveCurrentFormat() {
      StereoProjectionFormat format = savedStereoProjectionDetector.SavedFormat;

      if (CompareFormat(format)) {
        return;
      }

      if (format == null) {
        format = new StereoProjectionFormat();
      }

      Debug.Log("Saving Stereo Projection Format.");

      format.projectionMode = CurrentProjectionMode;
      format.stereoMode = CurrentStereoMode;
      format.frameAspectRatio = CurrentAspectRatio;
      savedStereoProjectionDetector.SavedFormat = format;
    }

    private bool CompareFormat(StereoProjectionFormat format) {
      if (format == null) {
        return false;
      }

      if (format.projectionMode != CurrentProjectionMode) {
        return false;
      }

      if (format.stereoMode != CurrentStereoMode) {
        return false;
      }

      if (format.frameAspectRatio != CurrentAspectRatio) {
        return false;
      }

      return true;
    }

    protected virtual void InitScreen() {
      if (MediaScreen == null) {
        CreateScreen();
      }

      SetupScreen();
    }

    protected virtual void CreateScreen() {
      DestroyScreen();

      GameObject screen;
      switch (CurrentProjectionMode) {
        case ProjectionMode.Flat:
          screen = GameObject.Instantiate(flatPrefab);
          break;
        case ProjectionMode.Projection180:
          screen = GameObject.Instantiate(Projection180Prefab);
          break;
        case ProjectionMode.Projection360:
          screen = GameObject.Instantiate(Projection360Prefab);
          break;
        default:
          screen = GameObject.Instantiate(flatPrefab);
          break;
      }

      screen.transform.SetParent(transform, false);
      MediaScreen = screen.GetComponent<Renderer>();

      // Make sure the aspect ratio gets updated for the new screen.
      CurrentAspectRatio = CurrentAspectRatio;

      Debug.Log("Created Media Screen.");
    }

    protected virtual void DestroyScreen() {
      if (MediaScreen != null) {
        Destroy(MediaScreen.gameObject);
        MediaScreen = null;
        Debug.Log("Destroyed Media Screen.");

      }
    }

    protected virtual void SetShaderParameters() {
      if (MediaScreen == null) {
        return;
      }

      MediaScreen.sharedMaterial.DisableKeyword("_STEREOMODE_TOPBOTTOM");
      MediaScreen.sharedMaterial.DisableKeyword("_STEREOMODE_LEFTRIGHT");

      switch (CurrentStereoMode) {
        case StereoMode.TopBottom:
          MediaScreen.sharedMaterial.EnableKeyword("_STEREOMODE_TOPBOTTOM");
          break;
        case StereoMode.LeftRight:
          MediaScreen.sharedMaterial.EnableKeyword("_STEREOMODE_LEFTRIGHT");
          break;
        default:
          break;
      }
    }

    protected virtual void SetupScreen() {
      if (MediaScreen == null) {
        return;
      }

      SetupScreenInternal();
      SetShaderParameters();
    }

    protected virtual void Awake() {
    }

    protected virtual void OnDestroy() {
      if (detectorQueue != null) {
        detectorQueue.ResetDetection();
      }
    }
  }
}
