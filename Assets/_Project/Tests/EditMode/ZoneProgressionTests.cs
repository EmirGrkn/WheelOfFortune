using NUnit.Framework;
using WheelOfFortune.Core;

namespace WheelOfFortune.Tests
{
    public class ZoneProgressionTests
    {
        private ZoneProgression _progression;

        [SetUp]
        public void SetUp()
        {
            _progression = new ZoneProgression(safeZoneInterval: 5, superZoneInterval: 30, rewardGrowthPerZone: 0.1f);
        }

        [TestCase(1, ZoneType.Normal)]
        [TestCase(4, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(25, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]
        [TestCase(60, ZoneType.Super)]
        public void GetZoneType_ReturnsExpectedType(int zone, ZoneType expected)
        {
            Assert.AreEqual(expected, _progression.GetZoneType(zone));
        }

        [TestCase(1, false)]
        [TestCase(5, true)]
        [TestCase(30, true)]
        public void IsRiskFree_OnlyForSafeAndSuperZones(int zone, bool expected)
        {
            Assert.AreEqual(expected, _progression.IsRiskFree(zone));
        }

        [TestCase(1, ZoneType.Safe, 5)]
        [TestCase(5, ZoneType.Safe, 10)]
        [TestCase(11, ZoneType.Safe, 15)]
        [TestCase(1, ZoneType.Super, 30)]
        [TestCase(30, ZoneType.Super, 60)]
        public void GetNextZone_ReturnsUpcomingZoneOfType(int zone, ZoneType type, int expected)
        {
            Assert.AreEqual(expected, _progression.GetNextZone(zone, type));
        }

        [TestCase(100, 1, 100)]
        [TestCase(100, 11, 200)]
        [TestCase(15, 2, 17)]
        public void ScaleRewardAmount_GrowsLinearlyPerZone(int baseAmount, int zone, int expected)
        {
            Assert.AreEqual(expected, _progression.ScaleRewardAmount(baseAmount, zone));
        }
    }
}
