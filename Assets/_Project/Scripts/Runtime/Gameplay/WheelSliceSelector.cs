using System;
using System.Collections.Generic;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    public static class WheelSliceSelector
    {
        /// <summary>
        /// Picks a slice index using slice weights. <paramref name="roll"/> must be in [0, 1).
        /// </summary>
        public static int SelectIndex(IReadOnlyList<WheelSliceData> slices, float roll)
        {
            float totalWeight = 0f;
            for (int i = 0; i < slices.Count; i++)
            {
                totalWeight += slices[i].Weight;
            }

            if (totalWeight <= 0f)
                throw new InvalidOperationException("Wheel has no slice with a positive weight.");

            float target = roll * totalWeight;
            float cumulative = 0f;
            int lastSelectable = -1;

            for (int i = 0; i < slices.Count; i++)
            {
                float weight = slices[i].Weight;
                if (weight <= 0f) continue;

                lastSelectable = i;
                cumulative += weight;
                if (target < cumulative) return i;
            }

            // Floating point error can leave target at the very end of the range.
            return lastSelectable;
        }
    }
}
