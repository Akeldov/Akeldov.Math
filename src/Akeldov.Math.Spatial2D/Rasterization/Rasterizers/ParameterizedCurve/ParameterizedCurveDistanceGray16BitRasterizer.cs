using System;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curves into 16-bit grayscale rasters using projection-to-curve mapping.
    /// </summary>
    public sealed class ParameterizedCurveDistanceGray16BitRasterizer : ISpatialRasterizer<IParameterizedCurve, Gray16BitColor>
    {
        private readonly Func<float, float, Gray16BitColor> _projectionToGrayLevel;

        /// <summary>
        /// Initializes a new parameterized curve rasterizer.
        /// </summary>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the curve and curve coordinate to a 16-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate.
        /// </param>
        public ParameterizedCurveDistanceGray16BitRasterizer(Func<float, float, Gray16BitColor> projectionToGrayLevel)
        {
            _projectionToGrayLevel = projectionToGrayLevel ?? throw new ArgumentNullException(nameof(projectionToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IParameterizedCurve source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sampler = new ParameterizedCurveRasterSampler<Gray16BitColor>(source, _projectionToGrayLevel);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }
    }
}
