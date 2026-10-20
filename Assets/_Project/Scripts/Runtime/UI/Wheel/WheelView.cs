using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelView : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_spin";
        private const float SliceAngle = 360f / WheelData.SliceCount;

        [Header("References")]
        [SerializeField] private RectTransform _rotatingRoot;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField, Tooltip("Clockwise, starting from the top.")]
        private WheelSliceView[] _slices = new WheelSliceView[WheelData.SliceCount];
        [SerializeField] private Sprite _bombIcon;
        [SerializeField, Min(0.1f), Tooltip("The bomb sprite has wide transparent margins, so it is scaled up to match other icons.")]
        private float _bombIconScale = 1.5f;

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Button _spinButton;

        [Header("Layout")]
        [SerializeField, Min(0f), Tooltip("Distance from the wheel center to the slice sockets.")]
        private float _sliceRadius = 144f;

        [Header("Spin Animation")]
        [SerializeField, Min(1)] private int _fullTurns = 5;
        [SerializeField, Min(0.1f)] private float _spinDuration = 3.5f;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;
        [SerializeField, Range(0f, 20f), Tooltip("Random offset so the wheel does not always stop at the exact slice center.")]
        private float _landingJitter = 12f;

        private Tween _spinTween;

        public event Action SpinClicked;

        private void OnEnable() => _spinButton.onClick.AddListener(HandleSpinClicked);

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _spinTween?.Kill();
        }

        public void SetWheel(WheelData wheel, Func<WheelSliceData, int> getAmount)
        {
            _baseImage.sprite = wheel.BaseSprite;
            _indicatorImage.sprite = wheel.IndicatorSprite;
            _titleText.text = wheel.Title;
            _titleText.color = wheel.TitleColor;

            for (int i = 0; i < _slices.Length; i++)
            {
                WheelSliceData slice = wheel.Slices[i];
                if (slice.IsBomb)
                {
                    _slices[i].Show(_bombIcon, string.Empty, _bombIconScale);
                }
                else
                {
                    _slices[i].Show(slice.Reward.Icon, "x" + AmountFormatter.FormatCompact(getAmount(slice)));
                }
            }
        }

        public void SetSpinInteractable(bool interactable) => _spinButton.interactable = interactable;

        public void PlaySpin(int sliceIndex, Action onComplete)
        {
            // Slice i sits at -i * SliceAngle, so rotating the root by +i * SliceAngle brings it under the indicator.
            // Subtracting full turns makes the wheel spin clockwise.
            float jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter);
            float targetAngle = sliceIndex * SliceAngle + jitter - 360f * _fullTurns;

            _spinTween?.Kill();
            _spinTween = _rotatingRoot
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .SetLink(gameObject)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void HandleSpinClicked() => SpinClicked?.Invoke();

        private void OnValidate()
        {
            _spinButton = ChildReferenceFinder.Find<Button>(this, SpinButtonName);

            if (_slices == null || _slices.Length != WheelData.SliceCount)
            {
                Array.Resize(ref _slices, WheelData.SliceCount);
            }
        }

        [ContextMenu("Arrange Slices")]
        private void ArrangeSlices()
        {
            for (int i = 0; i < _slices.Length; i++)
            {
                if (_slices[i] == null)
                {
                    Debug.LogWarning($"{name}: slice {i} is not assigned, skipped while arranging.", this);
                    continue;
                }

                float angle = i * SliceAngle;
                float radians = angle * Mathf.Deg2Rad;
                var sliceTransform = (RectTransform)_slices[i].transform;
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(sliceTransform, "Arrange Slices");
#endif
                sliceTransform.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * _sliceRadius;
                sliceTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }
        }
    }
}
