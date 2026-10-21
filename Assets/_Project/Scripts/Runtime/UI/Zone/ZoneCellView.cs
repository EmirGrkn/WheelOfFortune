using TMPro;
using UnityEngine;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneCellView : MonoBehaviour
    {
        private const string NumberTextName = "ui_text_zone_number_value";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private TMP_Text _numberText;

        public RectTransform RectTransform => (RectTransform)transform;

        public void Show(int zone, Color color)
        {
            _numberText.enabled = true;
            _numberText.text = zone.ToString();
            _numberText.color = color;
        }

        public void Hide() => _numberText.enabled = false;

        private void OnValidate()
        {
            _numberText = ChildReferenceFinder.Find<TMP_Text>(this, NumberTextName);
        }
    }
}
