using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Wheel
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        private const string IconImageName = "ui_image_slice_icon_value";
        private const string AmountTextName = "ui_text_slice_amount_value";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;

        public Vector3 IconPosition => _iconImage.rectTransform.position;

        /// <param name="iconScale">Uniform scale for icons whose sprite has a lot of transparent padding.</param>
        public void Show(Sprite icon, string amountText, float iconScale = 1f)
        {
            _iconImage.sprite = icon;
            _iconImage.rectTransform.localScale = Vector3.one * iconScale;
            _amountText.text = amountText;
            _amountText.enabled = !string.IsNullOrEmpty(amountText);
        }

        private void OnValidate()
        {
            _iconImage = ChildReferenceFinder.Find<Image>(this, IconImageName);
            _amountText = ChildReferenceFinder.Find<TMP_Text>(this, AmountTextName);
        }
    }
}
