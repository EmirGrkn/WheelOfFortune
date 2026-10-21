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

        [Header("Indicator Tick")]
        [SerializeField, Range(0f, 45f), Tooltip("How far the indicator swings when a slice border passes under it.")]
        private float _tickAngle = 18f;
        [SerializeField, Min(0f)] private float _tickReturnDuration = 0.15f;

        private Tween _spinTween;
        private Tween _tickTween;
        // Unwrapped wheel angle. localEulerAngles wraps at 360, which would break counting slice borders.
        private float _wheelAngle;
        private int _lastBorderIndex;

        public event Action SpinClicked;

        private void Awake() => _wheelAngle = _rotatingRoot.localEulerAngles.z;

        private void OnEnable() => _spinButton.onClick.AddListener(HandleSpinClicked);

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _spinTween?.Kill();
            _tickTween?.Kill(complete: true);
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

        public Vector3 GetSliceIconPosition(int sliceIndex) => _slices[sliceIndex].IconPosition;

        public void PlaySpin(int sliceIndex, Action onComplete)
        {
            // Slice i sits at -i * SliceAngle, so a wheel angle of i * SliceAngle brings it under the indicator.
            // The wheel turns clockwise (decreasing angle): first to the landing angle, then the extra full turns.
            float jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter);
            float landingAngle = sliceIndex * SliceAngle + jitter;
            float toLanding = Mathf.Repeat(_wheelAngle - landingAngle, 360f);
            float targetAngle = _wheelAngle - toLanding - 360f * _fullTurns;
            _lastBorderIndex = GetBorderIndex(_wheelAngle);

            _spinTween?.Kill();
            _spinTween = DOTween.To(() => _wheelAngle, SetWheelAngle, targetAngle, _spinDuration)
                .SetEase(_spinEase)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    // Keep the stored angle small; the visible rotation is the same.
                    _wheelAngle = Mathf.Repeat(_wheelAngle, 360f);
                    onComplete?.Invoke();
                });
        }

        private void SetWheelAngle(float angle)
        {
            _wheelAngle = angle;
            _rotatingRoot.localRotation = Quaternion.Euler(0f, 0f, angle);

            int borderIndex = GetBorderIndex(angle);
            if (borderIndex == _lastBorderIndex) return;

            _lastBorderIndex = borderIndex;
            PlayIndicatorTick();
        }

        /// <summary>Counts slice borders, which sit half a slice away from slice centers.</summary>
        private static int GetBorderIndex(float angle) => Mathf.FloorToInt((angle + SliceAngle * 0.5f) / SliceAngle);

        private void PlayIndicatorTick()
        {
            // The indicator pivots at its top, so a positive angle swings its tip to the right,
            // the direction the wheel rim moves at the top while spinning clockwise.
            RectTransform indicator = _indicatorImage.rectTransform;
            _tickTween?.Kill();
            indicator.localRotation = Quaternion.Euler(0f, 0f, _tickAngle);
            _tickTween = indicator.DOLocalRotate(Vector3.zero, _tickReturnDuration)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
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
