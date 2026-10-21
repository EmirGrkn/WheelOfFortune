using DG.Tweening;
using TMPro;
using UnityEngine;

namespace WheelOfFortune.UI.Zone
{
    /// <summary>Card showing the next safe or super zone number.</summary>
    public sealed class NextZoneCardView : MonoBehaviour
    {
        private const string NumberTextName = "ui_text_zone_card_number_value";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private TMP_Text _numberText;

        [Header("Number Punch")]
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField, Min(0f)] private float _punchDuration = 0.35f;

        private int _shownZone;

        public void SetZone(int zone)
        {
            if (zone == _shownZone) return;

            // The first value is shown without animation; later changes punch to draw attention.
            bool animate = _shownZone != 0;
            _shownZone = zone;
            _numberText.text = zone.ToString();
            if (!animate) return;

            Transform textTransform = _numberText.transform;
            textTransform.DOKill(complete: true);
            textTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, vibrato: 6).SetLink(gameObject);
        }

        private void OnDisable() => _numberText.transform.DOKill(complete: true);

        private void OnValidate()
        {
            _numberText = ChildReferenceFinder.Find<TMP_Text>(this, NumberTextName);
        }
    }
}
