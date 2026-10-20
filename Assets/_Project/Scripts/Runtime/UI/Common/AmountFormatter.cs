using System;
using System.Globalization;

namespace WheelOfFortune.UI
{
    public static class AmountFormatter
    {
        private static readonly (int Threshold, string Suffix)[] CompactUnits =
        {
            (1_000_000_000, "B"),
            (1_000_000, "M"),
            (1_000, "K")
        };

        /// <summary>Short form for small spaces, e.g. 1500 -> "1.5K". Truncates so the value is never overstated.</summary>
        public static string FormatCompact(int amount)
        {
            foreach ((int threshold, string suffix) in CompactUnits)
            {
                if (amount < threshold) continue;

                double value = Math.Floor(amount * 10d / threshold) / 10d;
                return value.ToString("0.#", CultureInfo.InvariantCulture) + suffix;
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Full form with thousands separators, e.g. 1634 -> "1,634".</summary>
        public static string FormatFull(int amount) => amount.ToString("N0", CultureInfo.InvariantCulture);
    }
}
