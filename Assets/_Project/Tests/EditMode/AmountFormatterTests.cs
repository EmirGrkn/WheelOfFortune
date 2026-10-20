using NUnit.Framework;
using WheelOfFortune.UI;

namespace WheelOfFortune.Tests
{
    public class AmountFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(999, "999")]
        [TestCase(1000, "1K")]
        [TestCase(1500, "1.5K")]
        [TestCase(1599, "1.5K")]
        [TestCase(10000, "10K")]
        [TestCase(999999, "999.9K")]
        [TestCase(2500000, "2.5M")]
        public void FormatCompact_UsesUnitSuffixes(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.FormatCompact(amount));
        }

        [TestCase(1634, "1,634")]
        [TestCase(12, "12")]
        public void FormatFull_UsesThousandsSeparator(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.FormatFull(amount));
        }
    }
}
