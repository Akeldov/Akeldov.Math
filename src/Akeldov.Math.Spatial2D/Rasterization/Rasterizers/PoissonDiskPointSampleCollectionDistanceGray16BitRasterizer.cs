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
        IRasterizer<IReadOnlyList<PoissonDiskPointSample>, Raster<ushort>>
    {
        private readonly Func<PoissonDiskPointSample, float, ushort> _sampleDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new Poisson disk point sample rasterizer.
        /// </summary>
        /// <param name="sampleDistanceToGrayLevel">
        /// The function that maps the nearest sample and distance to that sample, in world coordinate units,
        /// to a 16-bit grayscale value.
        /// </param>
        public PoissonDiskPointSampleCollectionDistanceGray16BitRasterizer(
            Func<PoissonDiskPointSample, float, ushort> sampleDistanceToGrayLevel)
        {
            _sampleDistanceToGrayLevel = sampleDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(sampleDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public Raster<ushort> Rasterize(IReadOnlyList<PoissonDiskPointSample> source, SpatialRasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            PoissonDiskPointSample[] samples = CopySamples(source);
            var values = new ushort[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                int valueIndex = y * grid.Resolution.X;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    PoissonDiskPointSample nearestSample = FindNearestSample(samples, point, out float distance);
                    values[valueIndex++] = _sampleDistanceToGrayLevel(nearestSample, distance);
                }
            }

            return new Raster<ushort>(grid, values);
        }

        private static PoissonDiskPointSample FindNearestSample(
            PoissonDiskPointSample[] samples,
            PointXY point,
            out float distance)
        {
            PoissonDiskPointSample nearestSample = samples[0];
            float nearestDistanceSquared = DistanceSquared(point, nearestSample.Point);

            for (int i = 1; i < samples.Length; i++)
            {
                PoissonDiskPointSample sample = samples[i];
                float distanceSquared = DistanceSquared(point, sample.Point);
                if (distanceSquared < nearestDistanceSquared)
                {
                    nearestSample = sample;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            distance = MathF.Sqrt(nearestDistanceSquared);
            return nearestSample;
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
