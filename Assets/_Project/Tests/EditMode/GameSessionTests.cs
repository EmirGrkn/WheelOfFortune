using System;
using NUnit.Framework;
using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class GameSessionTests
    {
        // Roll 0 lands on slice 0 (reward), roll 0.99 lands on the last slice (bomb).
        private const float RewardRoll = 0f;
        private const float BombRoll = 0.99f;

        private RewardItemData _gold;
        private WheelSliceData[] _slices;
        private FixedRandomProvider _random;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _gold = ScriptableObject.CreateInstance<RewardItemData>();
            _slices = new[]
            {
                new WheelSliceData(_gold, baseAmount: 100),
                WheelSliceData.CreateBomb()
            };
            _random = new FixedRandomProvider(RewardRoll);
            var progression = new ZoneProgression(safeZoneInterval: 5, superZoneInterval: 30, rewardGrowthPerZone: 0.1f);
            _session = new GameSession(progression, _random);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gold);
        }

        private void SpinAndComplete()
        {
            _session.Spin(_slices);
            _session.CompleteSpin();
        }

        [Test]
        public void NewSession_StartsAtFirstZoneReadyToSpin()
        {
            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(SessionState.ReadyToSpin, _session.State);
            Assert.IsTrue(_session.Inventory.IsEmpty);
        }

        [Test]
        public void Spin_ReturnsOutcomeAndWaitsForCompletion()
        {
            SpinOutcome outcome = _session.Spin(_slices);

            Assert.AreEqual(0, outcome.SliceIndex);
            Assert.IsFalse(outcome.IsBomb);
            Assert.AreEqual(SessionState.Spinning, _session.State);
            Assert.IsTrue(_session.Inventory.IsEmpty, "Reward must not be granted before the spin completes.");
        }

        [Test]
        public void CompleteSpin_OnReward_AddsScaledRewardAndAdvancesZone()
        {
            SpinAndComplete(); // zone 1: 100
            SpinAndComplete(); // zone 2: 110

            Assert.AreEqual(210, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(3, _session.CurrentZone);
            Assert.AreEqual(SessionState.ReadyToSpin, _session.State);
        }

        [Test]
        public void CompleteSpin_OnBomb_BustsAndKeepsRewardsUntilRestart()
        {
            SpinAndComplete();
            _random.Roll = BombRoll;

            SpinAndComplete();

            Assert.AreEqual(SessionState.Busted, _session.State);
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.IsFalse(_session.Inventory.IsEmpty, "Rewards are kept so the player can revive.");
        }

        [Test]
        public void Restart_AfterBomb_ClearsRewardsAndReturnsToFirstZone()
        {
            SpinAndComplete();
            _random.Roll = BombRoll;
            SpinAndComplete();

            _session.Restart();

            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(SessionState.ReadyToSpin, _session.State);
            Assert.IsTrue(_session.Inventory.IsEmpty);
        }

        [Test]
        public void Revive_AfterBomb_KeepsRewardsAndRetriesSameZone()
        {
            SpinAndComplete();
            _random.Roll = BombRoll;
            SpinAndComplete();

            _session.Revive();

            Assert.AreEqual(SessionState.ReadyToSpin, _session.State);
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
        }

        [Test]
        public void Spin_WhileSpinning_Throws()
        {
            _session.Spin(_slices);
            Assert.Throws<InvalidOperationException>(() => _session.Spin(_slices));
        }

        [Test]
        public void CanLeave_OnlyInSafeOrSuperZoneWhileNotSpinning()
        {
            Assert.IsFalse(_session.CanLeave, "Zone 1 is a normal zone.");

            for (int i = 0; i < 4; i++) SpinAndComplete();
            Assert.AreEqual(5, _session.CurrentZone);
            Assert.IsTrue(_session.CanLeave, "Zone 5 is a safe zone.");

            _session.Spin(_slices);
            Assert.IsFalse(_session.CanLeave, "Cannot leave while the wheel is spinning.");
        }

        [Test]
        public void Leave_InNormalZone_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _session.Leave());
        }

        [Test]
        public void Leave_InSafeZone_CollectsRewards()
        {
            for (int i = 0; i < 4; i++) SpinAndComplete();

            _session.Leave();

            Assert.AreEqual(SessionState.Collected, _session.State);
            Assert.IsFalse(_session.Inventory.IsEmpty);
        }
    }
}
