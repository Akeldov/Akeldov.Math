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
        /// Maps each chromatic-index triplet to a new value while preserving the raster geometry.
        /// </summary>
        /// <param name="raster">The source chromatic-index raster.</param>
        /// <param name="selector">The function that maps each chromatic-index triplet.</param>
        public static SpatialRaster<TResult> MapValues<TResult>(
            this ChromaticIndexTripletGrid raster,
            Func<Triplet<byte>, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            return raster.MapValues(raster.Geometry, selector);
        }
    }
}
