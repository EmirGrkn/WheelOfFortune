using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Rewards;

namespace WheelOfFortune.UI.Popups
{
    /// <summary>Shown when the player leaves and keeps the collected rewards.</summary>
    public sealed class CollectedPopupView : PopupView
    {
        private const string ContinueButtonName = "ui_button_continue";

        [Header("Rewards")]
        [SerializeField] private RewardRowView _rowPrefab;
        [SerializeField] private RectTransform _rowContainer;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Button _continueButton;

        // Rows are kept between runs and only the needed amount is enabled.
        private readonly List<RewardRowView> _rows = new List<RewardRowView>();

        public event Action ContinueClicked;

        private void OnEnable() => _continueButton.onClick.AddListener(HandleContinueClicked);

        private void OnDisable() => _continueButton.onClick.RemoveListener(HandleContinueClicked);

        public void SetRewards(RewardInventory inventory)
        {
            IReadOnlyList<RewardItemData> items = inventory.Items;

            while (_rows.Count < items.Count)
            {
                _rows.Add(Instantiate(_rowPrefab, _rowContainer));
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < items.Count;
                _rows[i].gameObject.SetActive(used);
                if (!used) continue;

                _rows[i].Show(items[i].Icon);
                _rows[i].SetAmount(inventory.GetAmount(items[i]), animate: false);
            }

            // Start from the top; the scroll position would otherwise be kept from the previous run.
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void HandleContinueClicked() => ContinueClicked?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            _continueButton = ChildReferenceFinder.Find<Button>(this, ContinueButtonName);
        }
    }
}
