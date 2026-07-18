using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class ChromaticIndexPartialTripletGridExtensions
    {
        /// <summary>
        /// Maps each partial chromatic-index triplet to a new value while preserving the raster geometry.
        /// </summary>
        /// <param name="raster">The source partial chromatic-index raster.</param>
        /// <param name="selector">The function that maps each partial chromatic-index triplet.</param>
        public static SpatialRaster<TResult> MapValues<TResult>(
            this ChromaticIndexPartialTripletGrid raster,
            Func<PartialTriplet<byte>, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            return raster.MapValues(raster.Geometry, selector);
        }
    }
}
