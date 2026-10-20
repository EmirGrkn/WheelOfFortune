using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public readonly struct SpinOutcome
    {
        public readonly int SliceIndex;
        public readonly bool IsBomb;
        public readonly RewardItemData Reward;
        public readonly int Amount;

        public SpinOutcome(int sliceIndex, bool isBomb, RewardItemData reward, int amount)
        {
            SliceIndex = sliceIndex;
            IsBomb = isBomb;
            Reward = reward;
            Amount = amount;
        }
    }
}
