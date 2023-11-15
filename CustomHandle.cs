using System;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class CustomHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public event Action<float> OnValueChanged;
    public event Action OnPointerDownAction;
    public event Action OnPointerUpAction;
    public event Action OnValueSet;

    [SerializeField] private RectTransform visualRect;
    private float _originalRectHeight;
    private bool _isOriginalRectHeightSet = true;

    private float _shrinkMultiplier = 200f;

    private float _tempHeightVal;
    private Vector3 _tempLocalPos;

    private bool _isDragging;
    private float _minY;
    private float _maxY;
    private Transform _transform;
    private float _touchOffset;

    private void Awake()
    {
        _originalRectHeight = visualRect.sizeDelta.y;
        _transform = transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _touchOffset = _transform.position.y - UIFrameHelper.GetUICameraToWorldPosition(Input.mousePosition).y;
        OnPointerDownAction?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _touchOffset = 0f;
        OnPointerUpAction?.Invoke();
    }

    public void SetMinMaxY(float minY, float maxY)
    {
        _minY = minY;
        _maxY = maxY;
        var position = transform.position;
        position.y = _maxY;
        transform.position = position;
    }

    private void Update()
    {
        if (_isDragging)
        {
            var position = transform.position;
            position.y = Mathf.Clamp(UIFrameHelper.GetUICameraToWorldPosition(Input.mousePosition).y + _touchOffset
                , _minY, _maxY);
            transform.position = position;
            OnValueChanged?.Invoke((position.y - _minY) / (_maxY - _minY));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void SetValue(float val)
    {
        if (_isDragging) return;

        OnValueSet?.Invoke();

        var position = _transform.position;
        position.y = Mathf.Lerp(_minY, _maxY, val);

        if (val > 1f)
        {
            // make handle smaller,  so it would look like it is shrinking
            _tempHeightVal = _originalRectHeight * (1f - (val - 1f) * _shrinkMultiplier);
            visualRect.sizeDelta = new Vector2(visualRect.sizeDelta.x, _tempHeightVal);
            _isOriginalRectHeightSet = false;
            _transform.position = position;
            _tempLocalPos = _transform.localPosition;
            _tempLocalPos.y += (_originalRectHeight - _tempHeightVal) * 0.5f;
            _transform.localPosition = _tempLocalPos;

        }
        else if (val < 0f)
        {
            // make handle smaller,  so it would look like it is shrinking
            _tempHeightVal = _originalRectHeight * (1f + val * _shrinkMultiplier);
            visualRect.sizeDelta = new Vector2(visualRect.sizeDelta.x, _tempHeightVal);
            _isOriginalRectHeightSet = false;
            _transform.position = position;
            _tempLocalPos = _transform.localPosition;
            _tempLocalPos.y -= (_originalRectHeight - _tempHeightVal) * 0.5f;
            _transform.localPosition = _tempLocalPos;
        }
        else if (!_isOriginalRectHeightSet)
        {
            visualRect.sizeDelta = new Vector2(visualRect.sizeDelta.x, _originalRectHeight);
            _isOriginalRectHeightSet = true;
            _transform.position = position;
        }
        else
        {
            _transform.position = position;
        }
    }

    public void SetSize(float size)
    {
        visualRect.sizeDelta = new Vector2(visualRect.sizeDelta.x, size);
        _originalRectHeight = visualRect.sizeDelta.y;
    }

    public void SetShrinkMultiplier(float shrinkMultiplier)
    {
        _shrinkMultiplier = shrinkMultiplier;
    }
}
