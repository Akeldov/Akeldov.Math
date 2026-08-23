using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curve collections into 8-bit grayscale rasters using nearest projection mapping.
    /// </summary>
    public sealed class ParameterizedCurveCollectionDistanceGray8BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<IParameterizedCurve>, Gray8BitColor>
    {
        private readonly Func<float, float, Gray8BitColor> _projectionToGrayLevel;

        /// <summary>
        /// Initializes a new parameterized curve collection rasterizer.
        /// </summary>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the nearest curve and its curve coordinate to an 8-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        public ParameterizedCurveCollectionDistanceGray8BitRasterizer(Func<float, float, Gray8BitColor> projectionToGrayLevel)
        {
            _projectionToGrayLevel = projectionToGrayLevel ?? throw new ArgumentNullException(nameof(projectionToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(IReadOnlyList<IParameterizedCurve> source, RasterGeometry grid)
        {
            ValidateSource(source);
            var sampler = new ParameterizedCurveCollectionRasterSampler<Gray8BitColor>(source, _projectionToGrayLevel);
            return SpatialRasterizationCore<Gray8BitColor>.Rasterize(grid, sampler, nameof(grid));
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
