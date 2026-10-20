using UnityEngine;
using WheelOfFortune.Core;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "zone_config", menuName = "Wheel Of Fortune/Zone Config")]
    public sealed class ZoneConfig : ScriptableObject
    {
        [Header("Zone Rules")]
        [SerializeField, Min(1)] private int _safeZoneInterval = 5;
        [SerializeField, Min(1)] private int _superZoneInterval = 30;
        [SerializeField, Min(0f), Tooltip("Reward multiplier added per zone. 0.1 means zone 11 gives x2 rewards.")]
        private float _rewardGrowthPerZone = 0.1f;

        [Header("Wheels")]
        [SerializeField] private WheelData _normalWheel;
        [SerializeField] private WheelData _safeWheel;
        [SerializeField] private WheelData _superWheel;

        public ZoneProgression CreateProgression() =>
            new ZoneProgression(_safeZoneInterval, _superZoneInterval, _rewardGrowthPerZone);

        public WheelData GetWheel(ZoneType type) => type switch
        {
            ZoneType.Safe => _safeWheel,
            ZoneType.Super => _superWheel,
            _ => _normalWheel
        };

        private void OnValidate()
        {
            if (_safeWheel != null && _safeWheel.HasBomb)
                Debug.LogWarning($"{name}: safe wheel '{_safeWheel.name}' contains a bomb, but safe zones must be risk-free.", this);
            if (_superWheel != null && _superWheel.HasBomb)
                Debug.LogWarning($"{name}: super wheel '{_superWheel.name}' contains a bomb, but super zones must be risk-free.", this);
        }
    }
}
