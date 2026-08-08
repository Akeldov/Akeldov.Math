using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides coherent-noise generation extensions for non-spatial raster resolutions.
    /// </summary>
    public static class VectorXYIntPerlinNoiseExtensions
    {
        /// <summary>
        /// Creates a floating-point raster by sampling fractal Perlin noise at the center of every
        /// cell in a unit-cell grid.
        /// </summary>
        /// <param name="resolution">The raster resolution. Both components must be positive.</param>
        /// <param name="seed">The deterministic seed used to select lattice gradients.</param>
        /// <param name="scale">
        /// The base feature scale in cell units. Larger values produce broader features.
        /// </param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">
        /// The amplitude multiplier between successive octaves, in the inclusive range [0, 1].
        /// </param>
        /// <param name="lacunarity">The positive frequency multiplier between successive octaves.</param>
        /// <param name="offset">The sample-space offset in cell units.</param>
        /// <returns>
        /// A raster whose value array is new, mutable, and owned by the caller. Every value is in
        /// the inclusive range [0, 1].
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a <paramref name="resolution"/> component is not positive or its cell count
        /// is too large, <paramref name="scale"/> is not finite and positive,
        /// <paramref name="octaves"/> is not positive, <paramref name="persistence"/> is not finite
        /// or lies outside [0, 1], <paramref name="lacunarity"/> is not finite and positive, or
        /// <paramref name="offset"/> contains a non-finite component.
        /// </exception>
        public static Raster<float> CreatePerlinNoise(
            this VectorXYInt resolution,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Raster resolution components must be positive.");

            long cellCount = (long)resolution.X * resolution.Y;
            if (cellCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Raster cell count must fit in a one-dimensional array.");

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

            float[] values = PerlinNoiseRasterGenerator.CreateValues(
                resolution,
                firstX: 0.5d,
                firstY: 0.5d,
                stepX: 1d,
                stepY: 1d,
                seed,
                scale,
                octaves,
                persistence,
                lacunarity,
                offset);

            return new Raster<float>(resolution, values);
        }
    }
}
