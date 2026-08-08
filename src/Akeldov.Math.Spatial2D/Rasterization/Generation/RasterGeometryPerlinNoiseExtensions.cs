using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides coherent-noise generation extensions for rectangular raster grids.
    /// </summary>
    public static class RasterGeometryPerlinNoiseExtensions
    {
        /// <summary>
        /// Creates a floating-point spatial raster by sampling fractal Perlin noise at the center
        /// of every raster cell.
        /// </summary>
        /// <param name="grid">The spatial raster grid to sample.</param>
        /// <param name="seed">The deterministic seed used to select lattice gradients.</param>
        /// <param name="scale">
        /// The base feature scale in world-coordinate units. Larger values produce broader features.
        /// </param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">
        /// The amplitude multiplier between successive octaves, in the inclusive range [0, 1].
        /// </param>
        /// <param name="lacunarity">The positive frequency multiplier between successive octaves.</param>
        /// <param name="offset">The sample-space offset in world-coordinate units.</param>
        /// <returns>
        /// A spatial raster whose value array is new, mutable, and owned by the caller. Every value
        /// is in the inclusive range [0, 1].
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="grid"/> is invalid or too large, <paramref name="scale"/> is
        /// not finite and positive, <paramref name="octaves"/> is not positive,
        /// <paramref name="persistence"/> is not finite or lies outside [0, 1],
        /// <paramref name="lacunarity"/> is not finite and positive, or <paramref name="offset"/>
        /// contains a non-finite component.
        /// </exception>
        public static SpatialRaster<float> CreatePerlinNoise(
            this RasterGeometry grid,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (float.IsNaN(grid.Origin.X) || float.IsInfinity(grid.Origin.X) ||
                float.IsNaN(grid.Origin.Y) || float.IsInfinity(grid.Origin.Y) ||
                !grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f ||
                grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(grid),
                    grid,
                    "Raster geometry must have finite bounds, positive size, and positive resolution components.");
            }

            long cellCount = (long)grid.Resolution.X * grid.Resolution.Y;
            if (cellCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(grid), grid, "Raster cell count must fit in a one-dimensional array.");

            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(scale), scale, "Noise scale must be finite and positive.");

            if (octaves <= 0)
                throw new ArgumentOutOfRangeException(nameof(octaves), octaves, "Octave count must be positive.");

            if (float.IsNaN(persistence) || float.IsInfinity(persistence) ||
                persistence < 0f || persistence > 1f)
                throw new ArgumentOutOfRangeException(nameof(persistence), persistence, "Persistence must be finite and lie in the inclusive range [0, 1].");

            if (float.IsNaN(lacunarity) || float.IsInfinity(lacunarity) || lacunarity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(lacunarity), lacunarity, "Lacunarity must be finite and positive.");

            if (!offset.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Noise offset components must be finite.");

            var values = new float[(int)cellCount];
            VectorXY cellSize = grid.CellSize;
            double firstX = grid.Origin.X + cellSize.X * 0.5d;
            double firstY = grid.Origin.Y + cellSize.Y * 0.5d;

            int valueIndex = 0;
            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                double pointY = firstY + y * (double)cellSize.Y;

                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    double pointX = firstX + x * (double)cellSize.X;
                    values[valueIndex++] = SampleFractalNoise(
                        pointX,
                        pointY,
                        seed,
                        scale,
                        octaves,
                        persistence,
                        lacunarity,
                        offset);
                }
            }

            return new SpatialRaster<float>(grid, values);
        }

        private static float SampleFractalNoise(
            double pointX,
            double pointY,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity,
            VectorXY offset)
        {
            double frequency = 1d / scale;
            double amplitude = 1d;
            double value = 0d;
            double amplitudeSum = 0d;

            for (int octave = 0; octave < octaves; octave++)
            {
                double sampleX = (pointX + offset.X) * frequency;
                double sampleY = (pointY + offset.Y) * frequency;

                value += PerlinNoise2D.Sample(sampleX, sampleY, seed) * amplitude;
                amplitudeSum += amplitude;
                amplitude *= persistence;

                if (amplitude == 0d)
                    break;

                frequency *= lacunarity;
            }

            double normalized = 0.5d + 0.5d * value / amplitudeSum;
            return (float)System.Math.Min(System.Math.Max(normalized, 0d), 1d);
        }
    }
}
