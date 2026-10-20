using NUnit.Framework;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class RewardInventoryTests
    {
        private RewardItemData _gold;
        private RewardItemData _cash;
        private RewardInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _gold = ScriptableObject.CreateInstance<RewardItemData>();
            _cash = ScriptableObject.CreateInstance<RewardItemData>();
            _inventory = new RewardInventory();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gold);
            Object.DestroyImmediate(_cash);
        }

        [Test]
        public void Add_SameItemTwice_SumsAmountsAndKeepsSingleEntry()
        {
            _inventory.Add(_gold, 10);
            _inventory.Add(_gold, 5);

            Assert.AreEqual(15, _inventory.GetAmount(_gold));
            Assert.AreEqual(1, _inventory.Items.Count);
        }

        [Test]
        public void Items_KeepFirstWonOrder()
        {
            _inventory.Add(_cash, 1);
            _inventory.Add(_gold, 1);
            _inventory.Add(_cash, 1);

            CollectionAssert.AreEqual(new[] { _cash, _gold }, _inventory.Items);
        }

        [Test]
        public void Add_RaisesItemChangedWithNewTotal()
        {
            int reportedTotal = 0;
            _inventory.ItemChanged += (item, total) => reportedTotal = total;

            _inventory.Add(_gold, 10);
            _inventory.Add(_gold, 5);

            Assert.AreEqual(15, reportedTotal);
        }

        [Test]
        public void Clear_RemovesEverythingAndRaisesCleared()
        {
            bool cleared = false;
            _inventory.Cleared += () => cleared = true;
            _inventory.Add(_gold, 10);

            _inventory.Clear();

            Assert.IsTrue(cleared);
            Assert.IsTrue(_inventory.IsEmpty);
            Assert.AreEqual(0, _inventory.GetAmount(_gold));
        }
    }
}
