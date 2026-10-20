using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "reward_", menuName = "Wheel Of Fortune/Reward Item")]
    public sealed class RewardItemData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
    }
}
