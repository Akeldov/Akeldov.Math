namespace Akeldov.Math.Hexes
{
    public static class SixfoldAngles
    {
        private static readonly SixfoldAngle[] AllValues = new SixfoldAngle[6]
        {
            SixfoldAngle.Deg0,
            SixfoldAngle.Deg60,
            SixfoldAngle.Deg120,
            SixfoldAngle.Deg180,
            SixfoldAngle.Deg240,
            SixfoldAngle.Deg300
        };

        /// <summary>
        /// Gets all sixfold angles in clockwise order.
        /// </summary>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static SixfoldAngle[] All => (SixfoldAngle[])AllValues.Clone();
    }
}
