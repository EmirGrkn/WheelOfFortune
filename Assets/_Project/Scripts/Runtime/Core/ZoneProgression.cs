using System;

namespace WheelOfFortune.Core
{
    /// <summary>
    /// Pure rules for zone types and reward growth. Kept free of Unity types so it can be unit tested.
    /// </summary>
    public sealed class ZoneProgression
    {
        public const int FirstZone = 1;

        private readonly int _safeZoneInterval;
        private readonly int _superZoneInterval;
        private readonly float _rewardGrowthPerZone;

        public ZoneProgression(int safeZoneInterval, int superZoneInterval, float rewardGrowthPerZone)
        {
            if (safeZoneInterval < 1) throw new ArgumentOutOfRangeException(nameof(safeZoneInterval));
            if (superZoneInterval < 1) throw new ArgumentOutOfRangeException(nameof(superZoneInterval));
            if (rewardGrowthPerZone < 0f) throw new ArgumentOutOfRangeException(nameof(rewardGrowthPerZone));

            _safeZoneInterval = safeZoneInterval;
            _superZoneInterval = superZoneInterval;
            _rewardGrowthPerZone = rewardGrowthPerZone;
        }

        public ZoneType GetZoneType(int zone)
        {
            // Super is checked first: zone 30 is both a multiple of 5 and 30, and super wins.
            if (zone % _superZoneInterval == 0) return ZoneType.Super;
            if (zone % _safeZoneInterval == 0) return ZoneType.Safe;
            return ZoneType.Normal;
        }

        public bool IsRiskFree(int zone) => GetZoneType(zone) != ZoneType.Normal;

        public int GetNextZone(int zone, ZoneType type)
        {
            int interval = type == ZoneType.Super ? _superZoneInterval : _safeZoneInterval;
            return (zone / interval + 1) * interval;
        }

        public int ScaleRewardAmount(int baseAmount, int zone)
        {
            float multiplier = 1f + _rewardGrowthPerZone * (zone - FirstZone);
            return (int)Math.Round(baseAmount * multiplier, MidpointRounding.AwayFromZero);
        }
    }
}
