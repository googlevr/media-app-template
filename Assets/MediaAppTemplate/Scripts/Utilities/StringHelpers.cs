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
using System.IO;
using System.Collections;
using System.Text;

namespace Daydream.MediaAppTemplate {

  /// Utilitiy functions for string manipulation.
  public static class StringHelpers {
    public static void GetNameWithoutExtension(FileInfo file, out StringBuilder name, out StringBuilder extension) {
      string fullName = file.FullName;
      int originalLength = fullName.Length;
      int startIndex = 0;
      int length = originalLength;
  
      int index = fullName.LastIndexOf('/');
      if (index != -1) {
        startIndex = index + 1;
        length = length - startIndex;
      }
  
      index = fullName.LastIndexOf('.');
      if (index != -1) {
        int difference = originalLength - index;
        length = length - difference;
        extension = new StringBuilder(fullName, index, difference, difference);
      } else {
        extension = null;
      }
  
      name = new StringBuilder(fullName, startIndex, length, length);
    }

    public static string TruncateStringWithEllipsis(string original, int maxLength) {
      if (original.Length <= maxLength) {
        return original;
      }
  
      string result = original.Substring(0, maxLength - 3);
      result += "...";
      return result;
    }

    public static void TruncateStringWithEllipsis(StringBuilder original, int maxLength) {
      if (original.Length <= maxLength) {
        return;
      }
  
      original.Length = maxLength;
      original[maxLength - 1] = '.';
      original[maxLength - 2] = '.';
      original[maxLength - 3] = '.';
    }
  }
}
