using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides coherent-noise generation extensions for hex-map geometries.
    /// </summary>
    public static class HexMapGeometryPerlinNoiseExtensions
    {
        /// <summary>
        /// Creates a floating-point map by sampling fractal Perlin noise at the center of every hex.
        /// </summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="seed">The deterministic seed used to select lattice gradients.</param>
        /// <param name="scale">
        /// The base feature scale in world-space units. Larger values produce
        /// broader features.
        /// </param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">
        /// The amplitude multiplier between successive octaves, in the inclusive range [0, 1].
        /// </param>
        /// <param name="lacunarity">The positive frequency multiplier between successive octaves.</param>
        /// <param name="offset">
        /// The sample-space offset in world-space units.
        /// </param>
        /// <returns>
        /// A new mutable floating-point map owned by the caller. Every value is in the inclusive
        /// range [0, 1]. The map retains the supplied geometry.
        /// </returns>
        /// <remarks>
        /// The sample position is the world-space hex center plus offset. Matching world positions
        /// use the same noise field for a given seed and noise parameters, independently of map bounds.
        /// Floating-point rounding may cause small differences when computing the same position by different paths.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry is invalid (including a default geometry),
        /// a sampled coordinate is outside the supported signed 64-bit noise lattice,
        /// <paramref name="scale"/> is not finite and positive,
        /// <paramref name="octaves"/> is not positive, <paramref name="persistence"/> is not finite
        /// or lies outside [0, 1], <paramref name="lacunarity"/> is not finite and positive, or
        /// <paramref name="offset"/> contains a non-finite component.
        /// </exception>
        public static SpatialFloatHexMap CreatePerlinNoise(
            this HexMapGeometry geometry,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");
            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

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

            var topology = geometry.Topology;
            var values = new float[topology.Count];

            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                int rowStart = y * topology.Resolution.X;

                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    VectorXY center = new VectorXYInt(x, y).GetHexCenter(geometry.Radius, geometry.Origin, topology.Layout);
                    values[rowStart + x] = SampleFractalNoise(
                        center,
                        seed,
                        scale,
                        octaves,
                        persistence,
                        lacunarity,
                        offset);
                }
            }

            return new SpatialFloatHexMap(geometry, values);
        }

        private static float SampleFractalNoise(
            VectorXY center,
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
                double sampleX = ((double)center.X + offset.X) * frequency;
                double sampleY = ((double)center.Y + offset.Y) * frequency;

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
