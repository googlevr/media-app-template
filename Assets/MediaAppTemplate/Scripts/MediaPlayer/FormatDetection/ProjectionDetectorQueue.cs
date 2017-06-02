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
#define DEBUG_PROJECTION_DETECTOR_QUEUE

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// Contains a list of projection detectors.
  /// Tries to detect the stereo projection format with each detector in order.
  /// Uses the first format that is successfully detected.
  public class ProjectionDetectorQueue : BaseStereoProjectionDetector {
    private class QueueRunner {
      private ProjectionDetectorQueue queue;
      private int currentIndex;

      public QueueRunner(ProjectionDetectorQueue queue) {
        this.queue = queue;
      }

      public void TryNextDetector() {
        if (currentIndex >= queue.detectors.Count) {
          queue.CompleteDetection(null);
          return;
        }
  
        BaseStereoProjectionDetector detector = queue.detectors[currentIndex];
        #if DEBUG_PROJECTION_DETECTOR_QUEUE
        Debug.Log("Attempting to detect Stereo Projection Format with " + detector);
        #endif
        detector.Detect(OnFormatDetected);
      }

      private void OnFormatDetected(StereoProjectionFormat format) {
        if (queue.queueRunner != this) {
          return;
        }
  
        if (format == null) {
          currentIndex++;
          TryNextDetector();
          return;
        }
  
        #if DEBUG_PROJECTION_DETECTOR_QUEUE
        Debug.Log("Detected Stereo Projection Format!");
        #endif
  
        queue.CompleteDetection(format);
      }
    }

    private List<BaseStereoProjectionDetector> detectors = new List<BaseStereoProjectionDetector>();
    private QueueRunner queueRunner;

    public ProjectionDetectorQueue(BaseMediaPlayer mediaPlayer)
      : base(mediaPlayer) {
    }

    public void AddDetector(BaseStereoProjectionDetector detector) {
      detectors.Add(detector);
    }

    protected override void DetectInternal() {
      if (detectors.Count == 0) {
        CompleteDetection(null);
        return;
      }
  
      queueRunner = new QueueRunner(this);
      queueRunner.TryNextDetector();
    }

    public override void ResetDetection() {
      base.ResetDetection();
      queueRunner = null;
  
      for (int i = 0; i < detectors.Count; i++) {
        BaseStereoProjectionDetector detector = detectors[i];
        detector.ResetDetection();
      }
    }
  }
}
