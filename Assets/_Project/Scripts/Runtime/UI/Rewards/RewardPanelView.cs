using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.UI.Rewards
{
    /// <summary>
    /// Left panel listing the rewards collected in the current run, plus the exit button.
    /// Rows are pooled so restarting a run does not instantiate them again.
    /// </summary>
    public sealed class RewardPanelView : MonoBehaviour
    {
        private const string ExitButtonName = "ui_button_exit";

        [Header("References")]
        [SerializeField] private RewardRowView _rowPrefab;
        [SerializeField] private RectTransform _rowContainer;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Auto-assigned in OnValidate")]
        [SerializeField] private Button _exitButton;

        private readonly Dictionary<RewardItemData, RewardRowView> _activeRows = new Dictionary<RewardItemData, RewardRowView>();
        private readonly Stack<RewardRowView> _pooledRows = new Stack<RewardRowView>();
        private RewardInventory _inventory;

        public event Action ExitClicked;

        private void OnEnable() => _exitButton.onClick.AddListener(HandleExitClicked);

        private void OnDisable() => _exitButton.onClick.RemoveListener(HandleExitClicked);

        private void OnDestroy() => Unbind();

        public void Bind(RewardInventory inventory)
        {
            Unbind();
            _inventory = inventory;
            _inventory.ItemChanged += HandleItemChanged;
            _inventory.Cleared += HandleCleared;

            foreach (RewardItemData item in _inventory.Items)
            {
                ShowItem(item, _inventory.GetAmount(item), animate: false);
            }
        }

        public void SetExitInteractable(bool interactable) => _exitButton.interactable = interactable;

        private void Unbind()
        {
            if (_inventory == null) return;

            _inventory.ItemChanged -= HandleItemChanged;
            _inventory.Cleared -= HandleCleared;
            _inventory = null;
        }

        private void HandleItemChanged(RewardItemData item, int total) => ShowItem(item, total, animate: true);

        private void HandleCleared()
        {
            foreach (RewardRowView row in _activeRows.Values)
            {
                row.gameObject.SetActive(false);
                _pooledRows.Push(row);
            }
            _activeRows.Clear();
        }

        private void ShowItem(RewardItemData item, int total, bool animate)
        {
            if (!_activeRows.TryGetValue(item, out RewardRowView row))
            {
                row = GetRow();
                row.Show(item.Icon);
                _activeRows.Add(item, row);
                ScrollToBottom();
            }

            row.SetAmount(total, animate);
        }

        private RewardRowView GetRow()
        {
            RewardRowView row = _pooledRows.Count > 0 ? _pooledRows.Pop() : Instantiate(_rowPrefab, _rowContainer);
            row.transform.SetAsLastSibling();
            row.gameObject.SetActive(true);
            return row;
        }

        private void ScrollToBottom()
        {
            // The layout group positions the new row on the next canvas update, so force it before scrolling.
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void HandleExitClicked() => ExitClicked?.Invoke();

        private void OnValidate()
        {
            _exitButton = ChildReferenceFinder.Find<Button>(this, ExitButtonName);
        }
    }
}
