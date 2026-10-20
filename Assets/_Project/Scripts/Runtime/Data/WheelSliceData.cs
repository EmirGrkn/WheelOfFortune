using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public sealed class WheelSliceData
    {
        [SerializeField] private bool _isBomb;
        [SerializeField] private RewardItemData _reward;
        [SerializeField, Min(1)] private int _baseAmount = 1;
        [SerializeField, Min(0f), Tooltip("Relative chance of landing on this slice.")]
        private float _weight = 1f;

        // Unity's serializer needs a parameterless constructor.
        public WheelSliceData() { }

        public WheelSliceData(RewardItemData reward, int baseAmount, float weight = 1f)
        {
            _reward = reward;
            _baseAmount = baseAmount;
            _weight = weight;
        }

        public static WheelSliceData CreateBomb(float weight = 1f) =>
            new WheelSliceData { _isBomb = true, _weight = weight };

        public bool IsBomb => _isBomb;
        public RewardItemData Reward => _reward;
        public int BaseAmount => _baseAmount;
        public float Weight => _weight;
    }
}
