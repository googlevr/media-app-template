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

#define DEBUG_IMAGE_DETECTOR

using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Daydream.MediaAppTemplate {

  /// Detects the stereo format of an image based upon
  /// analyzing the content of the image.
  public abstract class ImageBasedProjectionDetector : BaseStereoProjectionDetector {
    // This render texture is used to store a downsampled version of an image.
    // The downsampled version is then analyzed to determine the stereo mode by
    // evaluating the similarity of different parts of the image. Since the stereo frames
    // diverge based on the IPD of the image, the blurring caused by downsampling improves the
    // accuracy of the analysis.
    private RenderTexture renderTexture = null;

    // To get a good result from when downsampling the image, the source image must have mip maps.
    // This render texture is used to generate them if the source doesn't have any.
    private RenderTexture mipRenderTexture = null;

    private Coroutine analyzeImageCoroutine;

    /// Number of frames to wait after blitting to avoid pipeline stalls.
    private const  int FRAMES_AFTER_BLIT = 3;

    /// Height of the image after blitting.
    /// Width is variable based on aspect ratio.
    private const int IMAGE_HEIGHT = 16;

    /// Used to determine how similar two frames of an image must be
    /// for it to be considered a stereo image.
    private const float SIMILARITY_THRESHOLD = 0.025f;

    /// If the aspect ratio of a frame is larger than this value,
    /// then we assume it is a spherical image. This won't work in all cases,
    /// but is correct for the vast majority of content.
    private const float SPHERICAL_THRESHOLD = 1.9f;

    public ImageBasedProjectionDetector(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    protected void AnalyzeImage(Texture2D texture, Action<StereoProjectionFormat> callback) {
      analyzeImageCoroutine = MediaPlayer.StartCoroutine(AnalyzeImageCoroutine(texture, callback));
    }

    public override void ResetDetection() {
      base.ResetDetection();
      if (analyzeImageCoroutine != null) {
        MediaPlayer.StopCoroutine(analyzeImageCoroutine);
        analyzeImageCoroutine = null;
      }

      DisposeRenderTexture();
      DisposeMipRenderTexture();
    }

    // Override for to run custom logic after the image is analyzed occurs.
    protected virtual void PostAnalyzeImage() {
    }

    private IEnumerator AnalyzeImageCoroutine(Texture2D texture, Action<StereoProjectionFormat> callback) {
      // Wait a frame.
      yield return null;

      // Blit the texture to a tiny render texture.
      Blit(texture);

      // Wait a few frames before reading the pixels to avoid pipeline stalls
      // and reduce the amount of work done on a single frame.
      for (int i = 0; i < FRAMES_AFTER_BLIT; i++) {
        yield return new WaitForEndOfFrame();
      }

      // Calculate the format based on the blitted image.
      StereoProjectionFormat format = CalculateFormat(texture);

      // Invoke callback.
      callback(format);

      PostAnalyzeImage();
    }

    private void Blit(Texture2D texture) {
      if (renderTexture != null) {
        return;
      }

      Texture textureToDownsample;

      // To get a good result from when downsampling the image, the source image must have mip maps.
      // If the source doesn't have mip maps, blit it to a render texture to generate them.
      if (texture.mipmapCount == 1) {
        // Can't generate mip maps unless the render texture is a power of two.
        int widthPowerOfTwo = Mathf.ClosestPowerOfTwo(texture.width);
        int heightPowerOfTwo = Mathf.ClosestPowerOfTwo(texture.height);
        mipRenderTexture = RenderTexture.GetTemporary(widthPowerOfTwo, heightPowerOfTwo);
        mipRenderTexture.useMipMap = true;
        Graphics.Blit(texture, mipRenderTexture);
        textureToDownsample = mipRenderTexture;
      } else {
        textureToDownsample = texture;
      }

      int width = texture.width;
      int height = texture.height;
      float aspectRatio = (float)width / (float)height;
      width = (int)(IMAGE_HEIGHT * aspectRatio);
      height = IMAGE_HEIGHT;

      renderTexture = RenderTexture.GetTemporary(width, height);
      Graphics.Blit(textureToDownsample, renderTexture);

      // If it exists, dispose the Mip render texture.
      DisposeMipRenderTexture();
    }

    private StereoProjectionFormat CalculateFormat(Texture2D originalTexture) {
      if (renderTexture == null) {
        return null;
      }

      int width = renderTexture.width;
      int height = renderTexture.height;

      // Read the blitted texture.
      RenderTexture.active = renderTexture;
      Texture2D targetTexture = new Texture2D(width, height);
      targetTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
      DisposeRenderTexture();

      // Get the pixels from the texture.
      Color32[] pixels = targetTexture.GetPixels32();

      // Calculate the lightest and darkest color values in the image.
      // This is used to normalize the differences between pixels.
      Vector3 darkest;
      Vector3 lightest;
      CalculateLightestAndDarkestColors(pixels, out lightest, out darkest);

      int halfHeight = height / 2;
      int halfWidth = width / 2;

      #if DEBUG_IMAGE_DETECTOR
      Texture2D fullTex = new Texture2D(width, height, TextureFormat.RGB24, false);
      Texture2D leftRightTex = new Texture2D(halfWidth, height, TextureFormat.RGB24, false);
      Texture2D topBottomTex = new Texture2D(width, halfHeight, TextureFormat.RGB24, false);
      #endif

      Vector3 leftRightSum = Vector3.zero;
      Vector3 topBottomSum = Vector3.zero;

      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          int pixelIndex = LocationToIndex(x, y, width, height);
          Color pixel = pixels[pixelIndex];
          Vector3 normalizedPixel = NormalizeColorToRange(pixel, lightest, darkest);

          if (x < halfWidth) {
            int rightPixelIndex = LocationToIndex(x + halfWidth, y, width, height);
            Color rightPixel = pixels[rightPixelIndex];
            Vector3 normalizedRightPixel = NormalizeColorToRange(rightPixel, lightest, darkest);
            Vector3 diffPixel = normalizedPixel - normalizedRightPixel;

            leftRightSum += SquaredVector(diffPixel);
            #if DEBUG_IMAGE_DETECTOR
            leftRightTex.SetPixel(x, y, pixel - rightPixel);
            #endif
          }

          if (y < halfHeight) {
            int bottomPixelIndex = LocationToIndex(x, y + halfHeight, width, height);
            Color bottomPixel = pixels[bottomPixelIndex];
            Vector3 normalizedBottomPixel = NormalizeColorToRange(bottomPixel, lightest, darkest);
            Vector3 diffPixel = normalizedPixel - normalizedBottomPixel;

            topBottomSum += SquaredVector(diffPixel);
            #if DEBUG_IMAGE_DETECTOR
            topBottomTex.SetPixel(x, y, pixel - bottomPixel);
            #endif
          }

          #if DEBUG_IMAGE_DETECTOR
          fullTex.SetPixel(x, y, pixel);
          #endif
        }
      }

      #if DEBUG_IMAGE_DETECTOR
      string mediaPath = MediaPlayer.FilePath;
      mediaPath = Path.GetFileNameWithoutExtension(mediaPath);

      fullTex.Apply();
      byte[] bytes = fullTex.EncodeToPNG();
      string fullPath = Application.persistentDataPath + "/" + mediaPath + "Full.png";
      File.WriteAllBytes(fullPath, bytes);
      leftRightTex.Apply();
      bytes = leftRightTex.EncodeToPNG();
      string leftRightPath = Application.persistentDataPath + "/" + mediaPath + "LeftRightDiff.png";
      File.WriteAllBytes(leftRightPath, bytes);
      topBottomTex.Apply();
      bytes = topBottomTex.EncodeToPNG();
      string topBottomPath = Application.persistentDataPath + "/" + mediaPath + "TopBottomDiff.png";
      File.WriteAllBytes(topBottomPath, bytes);

      Debug.Log("Wrote Image Diffs." +
        "\nFull=" + fullPath +
        "\nLeftRight=" + leftRightPath +
        "\nTopBottom=" + topBottomPath);
      #endif

      // Left Right
      int numPixelsLeftRight = halfWidth * height;
      Vector3 leftRightRatio = leftRightSum / numPixelsLeftRight;
      float leftRightSimilarityRatio = AverageValueOfVector(leftRightRatio);

      // Top Bottom
      int numPixelsTopBottom = width * halfHeight;
      Vector3 topBottomRatio = topBottomSum / numPixelsTopBottom;
      float topBottomSimilarityRatio = AverageValueOfVector(topBottomRatio);

      #if DEBUG_IMAGE_DETECTOR
      Debug.Log("Left/Right similarity ratio = " + leftRightSimilarityRatio);
      Debug.Log("Top/Bottom similarity ratio = " + topBottomSimilarityRatio);
      #endif

      StereoProjectionFormat format = new StereoProjectionFormat();

      if (leftRightSimilarityRatio < SIMILARITY_THRESHOLD
          && leftRightSimilarityRatio < topBottomSimilarityRatio) {
        format.stereoMode = BaseMediaPlayer.StereoMode.LeftRight;
      } else if (topBottomSimilarityRatio < SIMILARITY_THRESHOLD) {
        format.stereoMode = BaseMediaPlayer.StereoMode.TopBottom;
      }

      format.frameAspectRatio =
        ImageBasedProjectionDetectorHelpers.CalculateFrameAspectRatio(originalTexture, format.stereoMode);

      if (format.frameAspectRatio >= SPHERICAL_THRESHOLD) {
        format.projectionMode = BaseMediaPlayer.ProjectionMode.Projection360;
      }

      #if DEBUG_IMAGE_DETECTOR
      Debug.Log("Frame Aspect Ratio = " + format.frameAspectRatio);
      #endif

      return format;
    }

    private void DisposeRenderTexture() {
      if (renderTexture != null) {
        RenderTexture.ReleaseTemporary(renderTexture);
        renderTexture = null;
      }
    }

    private void DisposeMipRenderTexture() {
      if (mipRenderTexture != null) {
        RenderTexture.ReleaseTemporary(mipRenderTexture);
        mipRenderTexture = null;
      }
    }

    /// Iterate through colors to find the darkest and lightest color values.
    private static void CalculateLightestAndDarkestColors(Color32[] colors, out Vector3 lightest, out Vector3 darkest) {
      darkest = Vector3.one;
      lightest = Vector3.zero;

      for (int i = 0; i < colors.Length; i++) {
        Color col = colors[i];
        Vector3 colVector = ColorToVector(col);
        darkest = Vector3.Min(darkest, colVector);
        lightest = Vector3.Max(lightest, colVector);
      }
    }

    /// Scale a color to a range while maintaining ratio.
    /// Returns result as a Vector.
    private static Vector3 NormalizeColorToRange(Color col, Vector3 min, Vector3 max) {
      Vector3 colVec = ColorToVector(col);

      Vector3 range = max - min;
      colVec -= min;

      if (range.x != 0.0f) {
        colVec.x /= range.x;
      }

      if (range.y != 0.0f) {
        colVec.y /= range.y;
      }

      if (range.z != 0.0f) {
        colVec.z /= range.z;
      }

      return colVec;
    }

    private static Vector3 ColorToVector(Color col) {
      return new Vector3(col.r, col.g, col.b);
    }

    private static Vector3 SquaredVector(Vector3 vec) {
      return Vector3.Scale(vec, vec);
    }

    private static float AverageValueOfVector(Vector3 vec) {
      return (vec.x + vec.y + vec.z) / 3.0f;
    }

    private static int LocationToIndex(int x, int y, int width, int height) {
      return y * width + x;
    }
  }
}
