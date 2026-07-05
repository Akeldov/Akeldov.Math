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
        ISpatialRasterizer<IReadOnlyList<PoissonDiskPointSample>, ushort>
    {
        private readonly float _pointRadius;
        private readonly float _ringThickness;
        private readonly ushort _backgroundGrayLevel;
        private readonly ushort _ringGrayLevel;
        private readonly ushort _pointGrayLevel;

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
            ushort backgroundGrayLevel,
            ushort ringGrayLevel,
            ushort pointGrayLevel)
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
        public SpatialRaster<ushort> Rasterize(IReadOnlyList<PoissonDiskPointSample> source, SpatialRasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            PoissonDiskPointSample[] samples = CopySamples(source);
            var values = new ushort[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float edgeFalloff = MathF.Max(cellSize.X, cellSize.Y) * 0.5f;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                int valueIndex = y * grid.Resolution.X;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    values[valueIndex++] = RasterizeCell(samples, point, edgeFalloff);
                }
            }

            return new SpatialRaster<ushort>(grid, values);
        }

        private ushort RasterizeCell(PoissonDiskPointSample[] samples, PointXY point, float edgeFalloff)
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

            ushort grayLevel = Blend(_backgroundGrayLevel, _ringGrayLevel, ringAmount);
            return Blend(grayLevel, _pointGrayLevel, pointAmount);
        }

        private static float GetCoverage(float distance, float radius, float edgeFalloff)
        {
            if (distance <= radius)
                return 1f;

            return 1f - MathF.Min(MathF.Max((distance - radius) / edgeFalloff, 0f), 1f);
        }

        private static ushort Blend(ushort from, ushort to, float amount)
        {
            amount = MathF.Min(MathF.Max(amount, 0f), 1f);
            float inverseAmount = 1f - amount;
            return (ushort)MathF.Round(from * inverseAmount + to * amount);
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

        private static void ValidateGrid(SpatialRasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }
    }
}
