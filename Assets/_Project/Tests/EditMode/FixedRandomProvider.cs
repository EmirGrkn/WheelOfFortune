using WheelOfFortune.Core;

namespace WheelOfFortune.Tests
{
    /// <summary>Test double that always returns the configured roll.</summary>
    public sealed class FixedRandomProvider : IRandomProvider
    {
        public float Roll { get; set; }

        public FixedRandomProvider(float roll)
        {
            Roll = roll;
        }

        public float NextFloat() => Roll;
    }
}
