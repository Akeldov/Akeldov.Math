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
        /// Maps each barycentric value to a new value while preserving the raster geometry.
        /// </summary>
        /// <param name="raster">The source barycentric raster.</param>
        /// <param name="selector">The function that maps each barycentric value.</param>
        public static SpatialRaster<TResult> MapValues<TResult>(
            this BarycentricTripletGrid raster,
            Func<Triplet<float>, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            return raster.MapValues(raster.Geometry, selector);
        }
    }
}
