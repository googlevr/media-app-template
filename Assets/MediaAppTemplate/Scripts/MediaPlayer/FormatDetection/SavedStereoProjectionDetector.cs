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
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Daydream.MediaAppTemplate {

  /// Attempts to detect the stereo proejction format of a media file
  /// based on data saved to disc associated with the media file.
  /// TODO: Make this work as an LRU cache so that the amount of saved formats can't grow indefinitely.
  public class SavedStereoProjectionDetector : BaseStereoProjectionDetector {
  
    private FileInfo formatFileInfo;
    private DirectoryInfo formatDirectoryInfo;
    private StereoProjectionFormat format;
  
    public const string SAVED_PATH_PREFIX = "/StereoProjectionFormat";
    public const string SAVED_FORMAT_EXTENSION = ".stereoFormat";

    public FileInfo SavedFormatFileInfo {
      get {
        if (formatFileInfo != null) {
          return formatFileInfo;
        }
  
        string formatPath = Application.persistentDataPath;
        formatPath += SAVED_PATH_PREFIX;
  
        string mediaPath = MediaPlayer.FilePath;
        int idx = mediaPath.LastIndexOf(DirectoryHelpers.GetHomeDirectory());
        if (idx != -1) {
          int finalIdx = idx + DirectoryHelpers.GetHomeDirectory().Length;
          mediaPath = mediaPath.Substring(finalIdx, mediaPath.Length - finalIdx);
        }
  
        formatPath += mediaPath;
        formatPath = Path.ChangeExtension(formatPath, SAVED_FORMAT_EXTENSION);
        formatFileInfo = new FileInfo(formatPath);
        return formatFileInfo;
      }
    }

    public DirectoryInfo SavedFormatDirectoryInfo {
      get {
        if (formatDirectoryInfo != null) {
          return formatDirectoryInfo;
        }
  
        FileInfo fileInfo = SavedFormatFileInfo;
        if (fileInfo != null) {
          formatDirectoryInfo = fileInfo.Directory;
        }
  
        return formatDirectoryInfo;
      }
    }

    public bool HasSavedFormat {
      get {
        return SavedFormatFileInfo.Exists;
      }
    }

    public StereoProjectionFormat SavedFormat {
      get {
        if (format == null && HasSavedFormat) {
          try {
            using (FileStream file = SavedFormatFileInfo.Open(FileMode.Open)) {
              BinaryFormatter bf = new BinaryFormatter();
              format = (StereoProjectionFormat)bf.Deserialize(file);
              Debug.Log("Read Stereo Projection Format from " + file.Name);
            }
          } catch (Exception e) {
            Debug.LogError(e);
          }
        }
  
        return format;
      }
      set {
        try {
          if (value != null) {
            DirectoryInfo directory = SavedFormatDirectoryInfo;
            if (!directory.Exists) {
              directory.Create();
            }
  
            using (FileStream file = SavedFormatFileInfo.Create()) {
              BinaryFormatter bf = new BinaryFormatter();
              bf.Serialize(file, value);
            }
          } else if (HasSavedFormat) {
            SavedFormatFileInfo.Delete();
          }
  
          format = value;
        } catch (Exception e) {
          Debug.LogError(e);
        }
      }
    }

    public SavedStereoProjectionDetector(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    public override void ResetDetection() {
      base.ResetDetection();
      formatFileInfo = null;
      formatDirectoryInfo = null;
      format = null;
    }

    protected override void DetectInternal() {
      CompleteDetection(SavedFormat);
    }
  }
}
