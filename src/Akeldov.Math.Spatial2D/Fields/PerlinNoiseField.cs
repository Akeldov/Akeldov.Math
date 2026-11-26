using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>Samples deterministic fractal Perlin noise at world-space points.</summary>
    public sealed class PerlinNoiseField : IFloatField
    {
        private readonly int _seed;
        private readonly double _baseFrequency;
        private readonly int _octaves;
        private readonly float _persistence;
        private readonly float _lacunarity;
        private readonly VectorXY _offset;

        /// <summary>Creates a noise field with a fixed seed and octave settings.</summary>
        /// <param name="seed">The seed used to select lattice gradients.</param>
        /// <param name="scale">The finite, positive base feature size in world-coordinate units.</param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">The amplitude multiplier between octaves, in [0, 1].</param>
        /// <param name="lacunarity">The finite, positive frequency multiplier between octaves.</param>
        /// <param name="offset">The finite offset added to each query point in world-coordinate units.</param>
        /// <exception cref="ArgumentOutOfRangeException">A noise setting is outside its specified range.</exception>
        public PerlinNoiseField(int seed, float scale, int octaves = 4, float persistence = 0.5f,
            float lacunarity = 2f, VectorXY offset = default)
        {
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

            _seed = seed;
            _baseFrequency = 1d / scale;
            _octaves = octaves;
            _persistence = persistence;
            _lacunarity = lacunarity;
            _offset = offset;
        }

        /// <summary>Gets the inclusive lower bound of the normalized noise, zero.</summary>
        public float Min => 0f;

        /// <summary>Gets the inclusive upper bound of the normalized noise, one.</summary>
        public float Max => 1f;

        /// <summary>Computes the normalized noise value at a world-space point.</summary>
        /// <param name="point">The finite world-space coordinates to sample.</param>
        /// <returns>The noise value in the inclusive range [0, 1].</returns>
        /// <exception cref="ArgumentOutOfRangeException">The point is non-finite or an octave's
        /// scaled coordinates exceed the supported signed 64-bit lattice range.</exception>
        public float Sample(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Noise sample coordinates must be finite.");
            return Sample(point.X, point.Y);
        }

        internal float Sample(double pointX, double pointY)
        {
            double frequency = _baseFrequency;
            double amplitude = 1d;
            double value = 0d;
            double amplitudeSum = 0d;

            for (int octave = 0; octave < _octaves; octave++)
            {
                double sampleX = (pointX + _offset.X) * frequency;
                double sampleY = (pointY + _offset.Y) * frequency;

                value += PerlinNoise2D.Sample(sampleX, sampleY, _seed) * amplitude;
                amplitudeSum += amplitude;
                amplitude *= _persistence;

                if (amplitude == 0d)
                    break;

                frequency *= _lacunarity;
            }

            double normalized = 0.5d + 0.5d * value / amplitudeSum;
            return (float)System.Math.Min(System.Math.Max(normalized, 0d), 1d);
        }
    }
}
