namespace WheelOfFortune.Core
{
    public interface IRandomProvider
    {
        /// <summary>Returns a value in the range [0, 1).</summary>
        float NextFloat();
    }
}
