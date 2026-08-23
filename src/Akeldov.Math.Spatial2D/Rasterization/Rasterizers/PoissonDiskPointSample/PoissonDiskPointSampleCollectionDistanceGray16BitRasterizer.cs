using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes Poisson disk point samples into a 16-bit grayscale raster using nearest-sample distance mapping.
    /// </summary>
    public sealed class PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<PoissonDiskPointSample>, Gray16BitColor>
    {
        private readonly Func<PoissonDiskPointSample, float, Gray16BitColor> _sampleDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new Poisson disk point sample rasterizer.
        /// </summary>
        /// <param name="sampleDistanceToGrayLevel">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit grayscale value.
        /// </param>
        public PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(
            Func<PoissonDiskPointSample, float, Gray16BitColor> sampleDistanceToGrayLevel)
        {
            _sampleDistanceToGrayLevel = sampleDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(sampleDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<PoissonDiskPointSample> source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RasterGeometryValidation.ValidateAndGetCellCount(grid, nameof(grid));
            PoissonDiskPointSample[] samples = CopySamples(source);
            var sampler = new PoissonDiskPointSampleCollectionDistanceRasterSampler<Gray16BitColor>(
                samples,
                _sampleDistanceToGrayLevel);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private static PoissonDiskPointSample[] CopySamples(IReadOnlyList<PoissonDiskPointSample> source)
        {
            if (source.Count == 0)
                throw new ArgumentException("Poisson disk point sample collection must not be empty.", nameof(source));

            var copy = new PoissonDiskPointSample[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                PoissonDiskPointSample sample = source[i];
                if (!PointXYValidation.IsFinite(sample.Point))
                    throw new ArgumentException("Poisson disk point sample coordinates must be finite.", nameof(source));

                if (sample.MinimalDistance <= 0f ||
                    float.IsNaN(sample.MinimalDistance) ||
                    float.IsInfinity(sample.MinimalDistance))
                {
                    throw new ArgumentException("Poisson disk point sample minimal distances must be finite and positive.", nameof(source));
                }

                copy[i] = sample;
            }

            return copy;
        }

    }
}
