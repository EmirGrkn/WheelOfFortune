using System;
using NUnit.Framework;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class WheelSliceSelectorTests
    {
        private static WheelSliceData[] CreateSlices(params float[] weights)
        {
            var slices = new WheelSliceData[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                slices[i] = new WheelSliceData(reward: null, baseAmount: 1, weight: weights[i]);
            }
            return slices;
        }

        [TestCase(0f, 0)]
        [TestCase(0.24f, 0)]
        [TestCase(0.25f, 1)]
        [TestCase(0.99f, 3)]
        public void SelectIndex_EqualWeights_SplitsRangeEvenly(float roll, int expected)
        {
            var slices = CreateSlices(1f, 1f, 1f, 1f);
            Assert.AreEqual(expected, WheelSliceSelector.SelectIndex(slices, roll));
        }

        [TestCase(0.74f, 0)]
        [TestCase(0.76f, 1)]
        public void SelectIndex_RespectsWeights(float roll, int expected)
        {
            var slices = CreateSlices(3f, 1f);
            Assert.AreEqual(expected, WheelSliceSelector.SelectIndex(slices, roll));
        }

        [Test]
        public void SelectIndex_SkipsZeroWeightSlices()
        {
            var slices = CreateSlices(0f, 1f, 0f);
            Assert.AreEqual(1, WheelSliceSelector.SelectIndex(slices, 0f));
            Assert.AreEqual(1, WheelSliceSelector.SelectIndex(slices, 0.999f));
        }

        [Test]
        public void SelectIndex_AllZeroWeights_Throws()
        {
            var slices = CreateSlices(0f, 0f);
            Assert.Throws<InvalidOperationException>(() => WheelSliceSelector.SelectIndex(slices, 0.5f));
        }
    }
}
