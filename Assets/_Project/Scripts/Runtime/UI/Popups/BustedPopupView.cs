using System;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Popups
{
    /// <summary>Shown when the wheel lands on the bomb.</summary>
    public sealed class BustedPopupView : PopupView
    {
        private const string GiveUpButtonName = "ui_button_give_up";

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Button _giveUpButton;

        public event Action GiveUpClicked;

        private void OnEnable() => _giveUpButton.onClick.AddListener(HandleGiveUpClicked);

        private void OnDisable() => _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);

        private void HandleGiveUpClicked() => GiveUpClicked?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            _giveUpButton = ChildReferenceFinder.Find<Button>(this, GiveUpButtonName);
        }
    }
}
