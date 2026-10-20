using System;

namespace WheelOfFortune.Core
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider() : this(Environment.TickCount) { }

        public SystemRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        public float NextFloat() => (float)_random.NextDouble();
    }
}
