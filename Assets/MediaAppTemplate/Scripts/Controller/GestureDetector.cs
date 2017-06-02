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

using System.Collections.Generic;
using UnityEngine;

namespace Daydream.MediaAppTemplate {

  // Gesture detector gets input and determines whether a gesture has happened.
  public class GestureDetector {
#region constants

    // A slop represents a small rectangular region around the first touch point of a gesture.
    // If the user does not move outside of the slop, no gesture is detected. Gestures start to be
    // detected when the user moves outside of the slop. Vertical distance from the border to the
    // center of slop.
    private const float SLOP_VERTICAL = 0.165f;
    // Horizontal distance from the border to the center of slop.
    private const float SLOP_HORIZONTAL = 0.125f;
    private const float DELTA = 1.0e-7f;
    private const float CUTOFF_HZ = 10.0f;
    private const float RC = (float)(1.0 / (2.0 * Mathf.PI * CUTOFF_HZ));
    private const float RIGHT = 28;
    private const float UP = 151;
    private const float LEFT = 224;
    private const float DOWN = 317;
    // Velocity threshold for swipes in each directions.
    private const float SWIPE_VELOCITY_THRESHOLD_UP = -.2f;
    private const float SWIPE_VELOCITY_THRESHOLD_RIGHT = .75f;
    private const float SWIPE_VELOCITY_THRESHOLD_LEFT = -.75f;
    private const float SWIPE_VELOCITY_THRESHOLD_DOWN = .45f;

#endregion constants

#region types

    public enum Type {
      SWIPE,
      // Finger moves quickly across touch pad.
      SCROLL_START,
      // Finger starts scrolling on touch pad.
      SCROLL_UPDATE,
      // Finger is in the process of scrolling.
      SCROLL_END
      // Finger stops scrolling.
    }

    public enum Direction {
      UP,
      DOWN,
      LEFT,
      RIGHT
    }

    public struct Gesture {
      public Type type;
      public Vector2 velocity;
      public Direction direction;
      public Vector2 displacement;
      public Vector2 initialTouchPos;
    }

    private enum State {
      WAITING,
      // Waiting for user to touch down.
      TOUCHING,
      // Touching the touch pad but not scrolling.
      SCROLLING
      // Scrolling on the touch pad.
    }

    private struct TouchPoint {
      public Vector2 position;
      public System.DateTime timestamp;
    }

    private struct TouchInfo {
      public TouchPoint touchPoint;
      public bool touchUp;
      // This is true for one frame when the touch ends.
      public bool touchDown;
      // This is true for one frame when the touch begins.
      public bool isTouching;
    }

#endregion types

#region members

    private TouchInfo touchInfo;
    private TouchPoint prevTouchPoint;
    private TouchPoint curTouchPoint;
    private TouchPoint initTouchPoint;

    private State state;
    private Vector2 overallVelocity;

    public List<Gesture> GestureList {
      get;
      private set;
    }

    public bool Verbose {
      get;
      set;
    }

#endregion members

    private Vector2 ClampTouchpadPosition(Vector2 pos) {
      float x = Mathf.Clamp01(pos.x);
      float y = Mathf.Clamp01(pos.y);
      return new Vector2(x, y);
    }

    public GestureDetector() {
      GestureList = new List<Gesture>(8);
      Reset();
    }

    public void Reset() {
      state = State.WAITING;
      prevTouchPoint = new TouchPoint();
      curTouchPoint = new TouchPoint();
      initTouchPoint = new TouchPoint();
      touchInfo = new TouchInfo();
      overallVelocity = Vector2.zero;
    }

    public void Update(Vector2 touchPos, bool isTouching, bool prevIsTouching) {
      bool touchUp = !isTouching && prevIsTouching;
      bool touchDown = isTouching && !prevIsTouching;
      // The reference implementation gets the timestamp from the controller state, but that isn't
      // part of the Unity SDK. Now should be close enough.
      Update(touchUp, touchDown, isTouching, touchPos, System.DateTime.Now);
    }

    // The argument order matches function defined for testing function in the GVR SDK, which allows
    // us to easily port their tests.
    public void Update(bool touchUp, bool touchDown, bool isTouching, Vector2 touchPos,
                     System.DateTime timestamp) {
      touchInfo.touchUp = touchUp;
      touchInfo.touchDown = touchDown;
      touchInfo.isTouching = isTouching;
      touchInfo.touchPoint.position = ClampTouchpadPosition(touchPos);
      touchInfo.touchPoint.timestamp = timestamp;
      UpdateGestureFromTouchInfo();
    }

    private void UpdateGestureFromTouchInfo() {
      // Clear the gesture list.
      GestureList.Clear();

      switch (state) {
      // User has not put finger on touch pad.
        case State.WAITING:
          HandleWaitingState();
          break;
      // User has not started a gesture (by moving out of slop).
        case State.TOUCHING:
          HandleDetectingState();
          break;
      // User is scrolling on touchpad.
        case State.SCROLLING:
          HandleScrollingState();
          break;
        default:
          Debug.LogFormat("Wrong gesture detector state: {0}", state);
          break;
      }
    }

    private bool UpdateCurrentTouchpoint() {
      if (touchInfo.isTouching || touchInfo.touchUp) {
        // Update the touch point when the touch position has changed.
        if (!ApproxEqual(curTouchPoint.position, touchInfo.touchPoint.position)) {
          prevTouchPoint = curTouchPoint;
          curTouchPoint = touchInfo.touchPoint;

          // Compute updated velocity.
          UpdateOverallVelocity();
          return true;
        }
      }
      return false;
    }

    private void HandleWaitingState() {
      // User puts finger on touch pad (or when the touch down for current gesture is missed,
      // initiate gesture from current touch point).
      if (touchInfo.touchDown || touchInfo.isTouching) {
        // Update initial touchpoint.
        initTouchPoint = touchInfo.touchPoint;
        // Update current touchpoint.
        curTouchPoint = touchInfo.touchPoint;
        state = State.TOUCHING;
      }
    }

    private void HandleDetectingState() {
      // user lifts up finger from touch pad.
      if (touchInfo.touchUp || !(touchInfo.isTouching)) {
        Reset();
        return;
      }

      // Touch position is changed and the touch point moves outside of slop.
      if (UpdateCurrentTouchpoint() && touchInfo.isTouching
        && !InSlop(touchInfo.touchPoint.position)) {
        state = State.SCROLLING;
        Gesture gesture = new Gesture();
        gesture.type = Type.SCROLL_START;
        UpdateGesture(ref gesture);
        GestureList.Add(gesture);
        if (Verbose) {
          Debug.Log("Gesture detection starts");
        }
      }
    }

    private void HandleScrollingState() {
      // Update current touch point.
      bool touchPositionChanged = UpdateCurrentTouchpoint();
      if (touchInfo.touchUp || !(touchInfo.isTouching)) {  // Gesture ends.
        Gesture scrollEnd = new Gesture();
        scrollEnd.type = Type.SCROLL_END;
        UpdateGesture(ref scrollEnd);
        GestureList.Add(scrollEnd);

        if (SwipeDetected()) {  // Gesture ends with a swipe.
          Gesture swipe = new Gesture();
          swipe.type = Type.SWIPE;
          UpdateGesture(ref swipe);

          // Set the displacement of swipe to zero.
          swipe.displacement = Vector2.zero;
          GestureList.Add(swipe);

          if (Verbose) {
            Debug.Log("A swipe gesture is detected.");
          }
        }
        Reset();
        if (Verbose) {
          Debug.Log("Gesture detection ends");
        }
      } else if (touchPositionChanged) {  // User continues scrolling and there is
        // a change in touch position.
        Gesture scrollUpdate = new Gesture();
        scrollUpdate.type = Type.SCROLL_UPDATE;
        UpdateGesture(ref scrollUpdate);
        GestureList.Add(scrollUpdate);
      }
    }

    private Direction GetGestureDirection() {
      float x, y, gesture_angle;
      x = overallVelocity.x;
      if (x == 0)
        x += DELTA;
      y = -overallVelocity.y;
      // Angle of the gesture in degrees counterclockwise from positive x-axis.
      gesture_angle = Mathf.Atan2(y, x) * 180 / Mathf.PI;
      if (gesture_angle < 0)
        gesture_angle += 360;

      if (gesture_angle < RIGHT) {
        return Direction.RIGHT;
      }
      if (gesture_angle < UP) {
        return Direction.UP;
      }
      if (gesture_angle < LEFT) {
        return Direction.LEFT;
      }
      if (gesture_angle < DOWN) {
        return Direction.DOWN;
      }
      return Direction.RIGHT;
    }

    private void UpdateGesture(ref Gesture gesture) {
      gesture.velocity = overallVelocity;
      gesture.direction = GetGestureDirection();
      gesture.displacement = curTouchPoint.position - prevTouchPoint.position;
      gesture.initialTouchPos = initTouchPoint.position;
    }

    private void UpdateOverallVelocity() {
      float duration = (float)(curTouchPoint.timestamp - prevTouchPoint.timestamp).TotalSeconds;

      // If the timestamp does not change, do not update velocity.
      if (duration < DELTA)
        return;

      Vector2 displacement = curTouchPoint.position - prevTouchPoint.position;

      Vector2 velocity = displacement / duration;

      float weight = duration / (RC + duration);

      overallVelocity = Vector2.Lerp(overallVelocity, velocity, weight);
    }

    private bool SwipeDetected() {
      return (overallVelocity.x > SWIPE_VELOCITY_THRESHOLD_RIGHT)
      || (overallVelocity.x < SWIPE_VELOCITY_THRESHOLD_LEFT)
      || (overallVelocity.y < SWIPE_VELOCITY_THRESHOLD_UP)
      || (overallVelocity.y > SWIPE_VELOCITY_THRESHOLD_DOWN);
    }

    private bool InSlop(Vector2 touchPosition) {
      return (Mathf.Abs(touchPosition.x - initTouchPoint.position.x) < SLOP_HORIZONTAL)
      && (Mathf.Abs(touchPosition.y - initTouchPoint.position.y) < SLOP_VERTICAL);
    }

    private bool ApproxEqual(Vector2 v1, Vector2 v2) {
      return (Mathf.Abs(v1.x - v2.x) < DELTA) && (Mathf.Abs(v1.y - v2.y) < DELTA);
    }
  }
}