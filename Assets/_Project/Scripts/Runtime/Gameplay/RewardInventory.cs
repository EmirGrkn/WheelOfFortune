using System;
using System.Collections.Generic;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    /// <summary>
    /// Rewards collected during a session, kept in the order they were first won.
    /// </summary>
    public sealed class RewardInventory
    {
        private readonly Dictionary<RewardItemData, int> _amounts = new Dictionary<RewardItemData, int>();
        private readonly List<RewardItemData> _items = new List<RewardItemData>();

        /// <summary>Raised with the item and its new total.</summary>
        public event Action<RewardItemData, int> ItemChanged;
        public event Action Cleared;

        public IReadOnlyList<RewardItemData> Items => _items;
        public bool IsEmpty => _items.Count == 0;

        public int GetAmount(RewardItemData item) =>
            _amounts.TryGetValue(item, out int amount) ? amount : 0;

        public void Add(RewardItemData item, int amount)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (!_amounts.ContainsKey(item))
            {
                _items.Add(item);
            }

            int total = GetAmount(item) + amount;
            _amounts[item] = total;
            ItemChanged?.Invoke(item, total);
        }

        public void Clear()
        {
            _amounts.Clear();
            _items.Clear();
            Cleared?.Invoke();
        }
    }
}
