using System;
using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.UI.Popups
{
    /// <summary>
    /// Base for modal popups: fades the whole popup in while the content panel scales up, and reverses it on hide.
    /// The popup only accepts input once fully shown, so buttons cannot be pressed twice during the animation.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupView : MonoBehaviour
    {
        private const string ContentName = "ui_panel_popup_content";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;

        [Header("Popup Animation")]
        [SerializeField, Min(0f), Tooltip("Wait before appearing, e.g. to let the player see the result first.")]
        private float _showDelay;
        [SerializeField, Min(0f)] private float _showDuration = 0.3f;
        [SerializeField, Min(0f)] private float _hideDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _hiddenScale = 0.8f;

        private Sequence _sequence;

        public void Show()
        {
            gameObject.SetActive(true);
            _sequence?.Kill();

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
            _content.localScale = Vector3.one * _hiddenScale;

            _sequence = DOTween.Sequence()
                .AppendInterval(_showDelay)
                .Append(_canvasGroup.DOFade(1f, _showDuration))
                .Join(_content.DOScale(1f, _showDuration).SetEase(Ease.OutBack))
                .OnComplete(() => _canvasGroup.interactable = true)
                .SetLink(gameObject);
        }

        public void Hide(Action onHidden = null)
        {
            _sequence?.Kill();
            _canvasGroup.interactable = false;

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, _hideDuration))
                .Join(_content.DOScale(_hiddenScale, _hideDuration).SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onHidden?.Invoke();
                })
                .SetLink(gameObject);
        }

        protected virtual void OnValidate()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _content = ChildReferenceFinder.Find<RectTransform>(this, ContentName);
        }
    }
}
