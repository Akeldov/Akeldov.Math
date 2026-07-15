using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class BarycentricPartialTripletGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricPartialTripletGrid grid,
            Func<PartialTriplet<float>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(grid.Geometry, barycentricCoordinatesToColor);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndexPartialTripletGrid">The ChromaticIndexPartialTripletGrid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricPartialTripletGrid grid,
            ChromaticIndexPartialTripletGrid chromaticIndexPartialTripletGrid,
            Func<PartialTriplet<float>, PartialTriplet<byte>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new TValue[checked(grid.Resolution.X * grid.Resolution.Y)];

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(grid[i], chromaticIndexPartialTripletGrid[i]);

            return new SpatialRaster<TValue>(grid.Geometry, values);
        }
    }
}
