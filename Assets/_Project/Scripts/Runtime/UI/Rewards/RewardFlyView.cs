using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Rewards
{
    /// <summary>
    /// Overlay that flies copies of a reward icon from the wheel to the rewards panel.
    /// Icons burst out around the start point, then jump to the target one after another.
    /// </summary>
    public sealed class RewardFlyView : MonoBehaviour
    {
        [SerializeField] private Image _iconPrefab;

        [Header("Animation")]
        [SerializeField, Min(1)] private int _iconCount = 5;
        [SerializeField, Min(0f), Tooltip("Radius of the burst around the start point.")]
        private float _burstRadius = 50f;
        [SerializeField, Min(0f)] private float _burstDuration = 0.2f;
        [SerializeField, Min(0f)] private float _stagger = 0.06f;
        [SerializeField, Min(0.05f)] private float _flyDuration = 0.5f;
        [SerializeField] private float _jumpPower = 120f;
        [SerializeField, Range(0f, 1f)] private float _arrivalScale = 0.6f;

        private readonly Stack<Image> _pooledIcons = new Stack<Image>();

        /// <param name="onComplete">Invoked once, when the last icon arrives.</param>
        public void Play(Sprite icon, Vector3 fromWorld, Vector3 toWorld, Action onComplete)
        {
            Vector3 from = transform.InverseTransformPoint(fromWorld);
            Vector3 to = transform.InverseTransformPoint(toWorld);
            int remaining = _iconCount;

            for (int i = 0; i < _iconCount; i++)
            {
                Image image = GetIcon();
                image.sprite = icon;

                RectTransform iconTransform = image.rectTransform;
                iconTransform.localPosition = from;
                iconTransform.localScale = Vector3.zero;
                Vector3 burstPosition = from + (Vector3)(UnityEngine.Random.insideUnitCircle * _burstRadius);

                DOTween.Sequence()
                    .Append(iconTransform.DOScale(1f, _burstDuration).SetEase(Ease.OutBack))
                    .Join(iconTransform.DOLocalMove(burstPosition, _burstDuration).SetEase(Ease.OutQuad))
                    .AppendInterval(i * _stagger)
                    .Append(iconTransform.DOLocalJump(to, _jumpPower, 1, _flyDuration).SetEase(Ease.InQuad))
                    .Join(iconTransform.DOScale(_arrivalScale, _flyDuration).SetEase(Ease.InQuad))
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        ReleaseIcon(image);
                        remaining--;
                        if (remaining == 0) onComplete?.Invoke();
                    });
            }
        }

        private Image GetIcon()
        {
            Image image = _pooledIcons.Count > 0 ? _pooledIcons.Pop() : Instantiate(_iconPrefab, transform);
            image.gameObject.SetActive(true);
            return image;
        }

        private void ReleaseIcon(Image image)
        {
            image.gameObject.SetActive(false);
            _pooledIcons.Push(image);
        }
    }
}
