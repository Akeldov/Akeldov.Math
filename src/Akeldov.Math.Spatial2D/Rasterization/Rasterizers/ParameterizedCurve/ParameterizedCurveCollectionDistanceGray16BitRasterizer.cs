using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curve collections into 16-bit grayscale rasters using nearest projection mapping.
    /// </summary>
    public sealed class ParameterizedCurveCollectionDistanceGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<IParameterizedCurve>, Gray16BitColor>
    {
        private readonly Func<float, float, Gray16BitColor> _projectionToGrayLevel;

        /// <summary>
        /// Initializes a new parameterized curve collection rasterizer.
        /// </summary>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the nearest curve and its curve coordinate to a 16-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        public ParameterizedCurveCollectionDistanceGray16BitRasterizer(Func<float, float, Gray16BitColor> projectionToGrayLevel)
        {
            _projectionToGrayLevel = projectionToGrayLevel ?? throw new ArgumentNullException(nameof(projectionToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<IParameterizedCurve> source, RasterGeometry grid)
        {
            ValidateSource(source);
            var sampler = new ParameterizedCurveCollectionRasterSampler<Gray16BitColor>(source, _projectionToGrayLevel);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private static void ValidateSource(IReadOnlyList<IParameterizedCurve> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Parameterized curve collection must contain at least one curve.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Parameterized curve collection must not contain null curves.", nameof(source));
            }
        }

    }
}
