using System;
using DG.Tweening;
using Game.Enum;
using UI.Common;
using UnityEngine;


public class CustomScrollBar : MonoBehaviour
{
    [SerializeField] private CustomHandle handle;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private MirroredSlicedImage image;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite holdSprite;

    [SerializeField] private RectTransform top;
    [SerializeField] private RectTransform bottom;

    public event Action<float> OnValueChanged;

    private float _minSize = 200f;
    private bool _shouldHide;
    private float _lastVal;

    private bool _fadeTweenIsPlaying;
    private Tween _fadeTween;
    private const float _shrinkMultiplierConstant = 800f;

    private void Start()
    {
        handle.OnValueChanged += OnValueChanged;
        handle.OnPointerDownAction += OnPointerDown;
        handle.OnPointerUpAction += OnPointerUp;
        FadeOutScrollBar();
    }

    public void Initialize(float viewportHeight, float contentHeight)
    {
        var size = viewportHeight / contentHeight * viewportHeight;
        size = Mathf.Clamp(size, _minSize, viewportHeight);

        _shouldHide = size >= viewportHeight;

        if (_shouldHide)
        {
            HideScrollBar();
        }

        handle.SetSize(size);

        top.anchoredPosition = new Vector2(0, -size / 2);
        bottom.anchoredPosition = new Vector2(0, size / 2);

        handle.SetMinMaxY(bottom.position.y, top.position.y);

        handle.SetShrinkMultiplier(contentHeight / _shrinkMultiplierConstant);
    }

    public void SetValue(float val)
    {
        handle.SetValue(val);

        if (Math.Abs(_lastVal - val) < 2e-05)
        {
            FadeOutScrollBar();
        }
        else
        {
            ShowScrollBar();
        }
        _lastVal = val;
    }

    private void OnPointerDown()
    {
        HoldScrollBar();
        ShowScrollBar();
        HapticManager.FakeButtonClickRequested(ButtonClickType.ButtonClick);
    }

    private void OnPointerUp()
    {
        ReleaseScrollBar();
        FadeOutScrollBar();
        HapticManager.FakeButtonClickRequested(ButtonClickType.ButtonUnClick);
    }

    private void HoldScrollBar()
    {
        var sizeDelta = image.rectTransform.sizeDelta;
        sizeDelta = new Vector2(holdSprite.bounds.size.x * holdSprite.pixelsPerUnit, sizeDelta.y);
        image.rectTransform.sizeDelta = sizeDelta;
        image.sprite = holdSprite;
    }

    private void ReleaseScrollBar()
    {
        var sizeDelta = image.rectTransform.sizeDelta;
        sizeDelta = new Vector2(defaultSprite.bounds.size.x * holdSprite.pixelsPerUnit, sizeDelta.y);
        image.rectTransform.sizeDelta = sizeDelta;
        image.sprite = defaultSprite;
    }


    public void ShowScrollBar()
    {
        if (_fadeTweenIsPlaying)
        {
            _fadeTween.Kill();
            _fadeTweenIsPlaying = false;
        }

        if (_shouldHide) return;

        canvasGroup.alpha = 1;
    }

    private void HideScrollBar()
    {
        if (_fadeTweenIsPlaying)
        {
            _fadeTween.Kill();
            _fadeTweenIsPlaying = false;
        }
        canvasGroup.alpha = 0;
    }

    private void FadeOutScrollBar()
    {
        if (_fadeTweenIsPlaying)
        {
            return;
        }
        _fadeTween = canvasGroup.DOFade(0, 2).SetEase(Ease.InExpo);
        _fadeTweenIsPlaying = true;
    }
}
