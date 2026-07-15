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
        /// Maps each partial barycentric value to a new value while preserving the raster geometry.
        /// </summary>
        /// <param name="raster">The source partial barycentric raster.</param>
        /// <param name="selector">The function that maps each partial barycentric value.</param>
        public static SpatialRaster<TResult> MapValues<TResult>(
            this BarycentricPartialTripletGrid raster,
            Func<PartialTriplet<float>, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            return raster.Rasterize(raster.Geometry, selector);
        }

        /// <summary>
        /// Maps paired partial barycentric and chromatic-index values while preserving the raster geometry.
        /// </summary>
        /// <param name="raster">The source partial barycentric raster.</param>
        /// <param name="chromaticIndexRaster">The corresponding partial chromatic-index raster.</param>
        /// <param name="selector">The function that maps each pair of values.</param>
        public static SpatialRaster<TResult> MapValues<TResult>(
            this BarycentricPartialTripletGrid raster,
            ChromaticIndexPartialTripletGrid chromaticIndexRaster,
            Func<PartialTriplet<float>, PartialTriplet<byte>, TResult> selector)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var values = new TResult[checked(raster.Resolution.X * raster.Resolution.Y)];

            for (int i = 0; i < values.Length; i++)
                values[i] = selector(raster[i], chromaticIndexRaster[i]);

            return new SpatialRaster<TResult>(raster.Geometry, values);
        }
    }
}
