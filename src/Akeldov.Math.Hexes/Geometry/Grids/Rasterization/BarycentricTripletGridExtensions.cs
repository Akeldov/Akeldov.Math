using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class BarycentricTripletGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricTripletGrid grid,
            Func<Triplet<float>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(grid.Geometry, barycentricCoordinatesToColor);
        }
    }
}
