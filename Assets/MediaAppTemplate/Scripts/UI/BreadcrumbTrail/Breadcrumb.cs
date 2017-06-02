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
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace Daydream.MediaAppTemplate {

  /// This script is a UI component for a breadcrumb.
  /// A breadcrumb is used to show hierarchy between content.
  /// View _BreadcrumbTrail_ to see how it is used.
  [RequireComponent(typeof(Button))]
  public class Breadcrumb : MonoBehaviour, IPointerClickHandler {
    public event Action<BreadcrumbData> OnBreadcrumbClicked;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Image background;

    [SerializeField]
    private int maxNameLength = 13;

    [SerializeField]
    private float animationDurationSeconds = 0.25f;

    [SerializeField]
    private Sprite defaultSprite;

    [SerializeField]
    private Sprite firstSprite;

    private Button button;
    private BreadcrumbData data;

    private Coroutine animationOutCoroutine;
    private Coroutine animationInCoroutine;
    private float layoutWidth;

    public BreadcrumbData Data {
      get {
        return data;
      }
    }

    public bool IsAnimatingIn {
      get {
        return animationInCoroutine != null;
      }
    }

    public bool IsAnimatingOut {
      get {
        return animationOutCoroutine != null;
      }
    }

    public bool IsAnimatedIn { get; private set; }
    public bool IsAnimatedOut { get; private set; }

    public void Setup(BreadcrumbData breadcrumbData) {
      data = breadcrumbData;
      string truncatedString = StringHelpers.TruncateStringWithEllipsis(breadcrumbData.displayName, maxNameLength);
      nameText.text = truncatedString;
    }

    public void Refresh(int index, int breadcrumbCount) {
      bool isLastBreadcrumb = index == breadcrumbCount - 1;

      // Workaround for a bug in Unity where it fails to clear it's selection
      // state when it is set to not interactable.
      if (isLastBreadcrumb) {
        button.OnDeselect(null);
        button.OnPointerExit(null);
      }

      button.interactable = !isLastBreadcrumb;

      if (index == 0 && background.sprite != firstSprite) {
        background.sprite = firstSprite;
        RectTransform rectTransform = background.GetComponent<RectTransform>();
        rectTransform.sizeDelta = firstSprite.rect.max;
        Vector3 localPos = rectTransform.localPosition;
        localPos.x = defaultSprite.rect.width - firstSprite.rect.width;
        rectTransform.localPosition = localPos;
      } else if (index != 0 && background.sprite != defaultSprite) {
        background.sprite = defaultSprite;
        RectTransform rectTransform = background.GetComponent<RectTransform>();
        rectTransform.sizeDelta = defaultSprite.rect.max;
        Vector3 localPos = rectTransform.localPosition;
        localPos.x = 0.0f;
        rectTransform.localPosition = localPos;
      }
    }

    public void OnPointerClick(PointerEventData eventData) {
      if (!button.IsInteractable()) {
        return;
      }

      if (OnBreadcrumbClicked != null) {
        OnBreadcrumbClicked(data);
      }
    }

    public void AnimateIn() {
      ClearAnimation();
      animationInCoroutine = StartCoroutine(DoAnimation(true));
    }

    public void AnimateOut() {
      ClearAnimation();
      animationOutCoroutine = StartCoroutine(DoAnimation(false));
    }

    public void AnimateOutAndDestroy() {
      ClearAnimation();
      animationOutCoroutine = StartCoroutine(DoAnimation(false, true));
    }

    private void ClearAnimation() {
      IsAnimatedIn = false;
      IsAnimatedOut = false;

      if (animationOutCoroutine != null) {
        StopCoroutine(animationOutCoroutine);
      }

      if (animationInCoroutine != null) {
        StopCoroutine(animationInCoroutine);
      }
    }

    void Awake() {
      button = GetComponent<Button>();
      LayoutElement layout = GetComponent<LayoutElement>();
      layoutWidth = layout.minWidth;
    }

    private IEnumerator DoAnimation(bool animateIn, bool destroy=false) {
      LayoutElement layout = GetComponent<LayoutElement>();

      Vector3 startScale = Vector3.one;
      Vector3 endScale = Vector3.one;

      if (animateIn) {
        startScale.x = 0.0f;
      } else {
        endScale.x = 0.0f;
      }

      float animationElapsedSeconds = 0.0f;
      transform.localScale = startScale;
      if (animateIn) {
        layout.minWidth = 0.0f;
      }

      yield return null;

      while (animationElapsedSeconds < animationDurationSeconds) {
        animationElapsedSeconds += Time.deltaTime;
        animationElapsedSeconds = Mathf.Min(animationElapsedSeconds, animationDurationSeconds);

        float ratio = animationElapsedSeconds / animationDurationSeconds;
        transform.localScale = Vector3.Lerp(startScale, endScale, ratio);

        if (!animateIn) {
          ratio = 1.0f - ratio;
        }
        layout.minWidth = ratio * layoutWidth;

        yield return null;
      }

      if (!animateIn) {
        gameObject.SetActive(false);
      }

      if (destroy) {
        Destroy(gameObject);
      }

      if (animateIn) {
        IsAnimatedIn = true;
      } else {
        IsAnimatedOut = true;
      }

      animationOutCoroutine = null;
      animationInCoroutine = null;
    }
  }
}
