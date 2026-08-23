using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes Poisson disk point samples into a 16-bit grayscale raster with sample points and minimal-distance rings.
    /// </summary>
    public sealed class PoissonDiskPointSampleCollectionRingsGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<PoissonDiskPointSample>, Gray16BitColor>
    {
        private readonly float _pointRadius;
        private readonly float _ringThickness;
        private readonly Gray16BitColor _backgroundGrayLevel;
        private readonly Gray16BitColor _ringGrayLevel;
        private readonly Gray16BitColor _pointGrayLevel;

        /// <summary>
        /// Initializes a new Poisson disk point sample ring rasterizer.
        /// </summary>
        /// <param name="pointRadius">The rendered sample point radius, in world coordinate units.</param>
        /// <param name="ringThickness">The rendered minimal-distance ring thickness, in world coordinate units.</param>
        /// <param name="backgroundGrayLevel">The 16-bit grayscale value used away from sample points and rings.</param>
        /// <param name="ringGrayLevel">The 16-bit grayscale value used at each minimal-distance ring centerline.</param>
        /// <param name="pointGrayLevel">The 16-bit grayscale value used at each rendered sample point center.</param>
        public PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
            float pointRadius,
            float ringThickness,
            Gray16BitColor backgroundGrayLevel,
            Gray16BitColor ringGrayLevel,
            Gray16BitColor pointGrayLevel)
        {
            if (pointRadius <= 0f || float.IsNaN(pointRadius) || float.IsInfinity(pointRadius))
                throw new ArgumentOutOfRangeException(nameof(pointRadius), "Point radius must be finite and positive.");

            if (ringThickness <= 0f || float.IsNaN(ringThickness) || float.IsInfinity(ringThickness))
                throw new ArgumentOutOfRangeException(nameof(ringThickness), "Ring thickness must be finite and positive.");

            _pointRadius = pointRadius;
            _ringThickness = ringThickness;
            _backgroundGrayLevel = backgroundGrayLevel;
            _ringGrayLevel = ringGrayLevel;
            _pointGrayLevel = pointGrayLevel;
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<PoissonDiskPointSample> source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RasterGeometryValidation.ValidateAndGetCellCount(grid, nameof(grid));
            PoissonDiskPointSample[] samples = CopySamples(source);
            VectorXY cellSize = grid.CellSize;
            float edgeFalloff = MathF.Max(cellSize.X, cellSize.Y) * 0.5f;

            var sampler = new RingsRasterSampler(this, samples, edgeFalloff);
            return SpatialRasterizationCore<Gray16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private readonly struct RingsRasterSampler : ISpatialRasterSampler<Gray16BitColor>
        {
            private readonly PoissonDiskPointSampleCollectionRingsGray16BitRasterizer _rasterizer;
            private readonly PoissonDiskPointSample[] _samples;
            private readonly float _edgeFalloff;

            public RingsRasterSampler(
                PoissonDiskPointSampleCollectionRingsGray16BitRasterizer rasterizer,
                PoissonDiskPointSample[] samples,
                float edgeFalloff)
            {
                _rasterizer = rasterizer;
                _samples = samples;
                _edgeFalloff = edgeFalloff;
            }

            public Gray16BitColor Sample(PointXY point) =>
                _rasterizer.RasterizeCell(_samples, point, _edgeFalloff);
        }

        private Gray16BitColor RasterizeCell(PoissonDiskPointSample[] samples, PointXY point, float edgeFalloff)
        {
            float ringAmount = 0f;
            float pointAmount = 0f;

            for (int i = 0; i < samples.Length; i++)
            {
                PoissonDiskPointSample sample = samples[i];
                float distance = MathF.Sqrt(DistanceSquared(point, sample.Point));
                pointAmount = MathF.Max(pointAmount, GetCoverage(distance, _pointRadius, edgeFalloff));
                ringAmount = MathF.Max(ringAmount, GetCoverage(
                    MathF.Abs(distance - sample.MinimalDistance),
                    _ringThickness * 0.5f,
                    edgeFalloff));
            }

            Gray16BitColor grayLevel = Gray16BitColor.Blend(_backgroundGrayLevel, _ringGrayLevel, ringAmount);
            return Gray16BitColor.Blend(grayLevel, _pointGrayLevel, pointAmount);
        }

        private static float GetCoverage(float distance, float radius, float edgeFalloff)
        {
            if (distance <= radius)
                return 1f;

            return 1f - MathF.Min(MathF.Max((distance - radius) / edgeFalloff, 0f), 1f);
        }

        private static float DistanceSquared(PointXY left, PointXY right)
        {
            float dx = left.X - right.X;
            float dy = left.Y - right.Y;
            return dx * dx + dy * dy;
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
