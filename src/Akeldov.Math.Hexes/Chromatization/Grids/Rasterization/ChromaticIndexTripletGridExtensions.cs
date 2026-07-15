using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class ChromaticIndexTripletGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndicesToColor">The ChromaticIndicesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this ChromaticIndexTripletGrid grid,
            Func<Triplet<byte>, TValue> chromaticIndicesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(grid.Geometry, chromaticIndicesToColor);
        }
    }
}
