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

namespace Daydream.MediaAppTemplate {

  /// Provides helper functions for accessing the system directory.
  public static class DirectoryHelpers {
    public static string GetHomeDirectory() {
      #if UNITY_ANDROID && !UNITY_EDITOR
      return DirectoryHelpers.GetAndroidExternalStorageDirectory();
      #else
      return Directory.GetCurrentDirectory();
      #endif
    }
  
#if UNITY_ANDROID
    private static string androidExternalStorageDirectory;

    private static string GetAndroidExternalStorageDirectory() {
      if (!string.IsNullOrEmpty(androidExternalStorageDirectory)) {
        return androidExternalStorageDirectory;
      }
  
      AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
      IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID(jc.GetRawClass(), "getExternalStorageDirectory", "()Ljava/io/File;");
      IntPtr file = AndroidJNI.CallStaticObjectMethod(jc.GetRawClass(), getExternalStorageDirectoryMethod, new jvalue[] { });
      IntPtr getPathMethod = AndroidJNI.GetMethodID(AndroidJNI.GetObjectClass(file), "getPath", "()Ljava/lang/String;");
      IntPtr path = AndroidJNI.CallObjectMethod(file, getPathMethod, new jvalue[] { });
      androidExternalStorageDirectory = AndroidJNI.GetStringUTFChars(path);
      AndroidJNI.DeleteLocalRef(file);
      AndroidJNI.DeleteLocalRef(path);
  
      return androidExternalStorageDirectory;
    }

    private static string GetAndroidExternalStoragePublicDirectory(string type) {
      AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
      IntPtr typeFieldId = AndroidJNI.GetStaticFieldID(jc.GetRawClass(), type, "Ljava/lang/String;");
      string typeField = AndroidJNI.GetStaticStringField(jc.GetRawClass(), typeFieldId);
      IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID(jc.GetRawClass(), "getExternalStoragePublicDirectory", "(Ljava/lang/String;)Ljava/io/File;");
      jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[] { typeField });
      IntPtr file = AndroidJNI.CallStaticObjectMethod(jc.GetRawClass(), getExternalStorageDirectoryMethod, args);
      IntPtr getPathMethod = AndroidJNI.GetMethodID(AndroidJNI.GetObjectClass(file), "getPath", "()Ljava/lang/String;");
      IntPtr path = AndroidJNI.CallObjectMethod(file, getPathMethod, new jvalue[] { });
      string pathString = AndroidJNI.GetStringUTFChars(path);
      AndroidJNI.DeleteLocalRef(file);
      AndroidJNI.DeleteLocalRef(path);
  
      return pathString;
    }
#endif
  }
}
