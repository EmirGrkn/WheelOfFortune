using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Rewards
{
    public sealed class RewardRowView : MonoBehaviour
    {
        private const string IconImageName = "ui_image_reward_icon_value";
        private const string AmountTextName = "ui_text_reward_amount_value";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;

        [Header("Amount Punch")]
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField] private float _punchDuration = 0.35f;

        public Vector3 IconPosition => _iconImage.rectTransform.position;

        public void Show(Sprite icon)
        {
            _iconImage.sprite = icon;
            SetContentVisible(true);
        }

        /// <summary>Reserves the row's place in the list while its reward is still flying in.</summary>
        public void ShowPending(Sprite icon)
        {
            _iconImage.sprite = icon;
            SetContentVisible(false);
        }

        public void SetAmount(int amount, bool animate)
        {
            SetContentVisible(true);
            _amountText.text = AmountFormatter.FormatFull(amount);
            if (!animate) return;

            Transform textTransform = _amountText.transform;
            textTransform.DOKill(complete: true);
            textTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, vibrato: 6).SetLink(gameObject);
        }

        private void SetContentVisible(bool visible)
        {
            _iconImage.enabled = visible;
            _amountText.enabled = visible;
        }

        private void OnDisable()
        {
            _amountText.transform.DOKill(complete: true);
        }

        private void OnValidate()
        {
            _iconImage = ChildReferenceFinder.Find<Image>(this, IconImageName);
            _amountText = ChildReferenceFinder.Find<TMP_Text>(this, AmountTextName);
        }
    }
}
