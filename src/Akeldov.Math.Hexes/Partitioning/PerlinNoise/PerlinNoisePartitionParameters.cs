using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Defines immutable noise settings for full and partial Perlin-noise partitions.</summary>
    public sealed class PerlinNoisePartitionParameters
    {
        /// <summary>Initializes the noise settings used to sample world-space hex centers.</summary>
        /// <param name="scale">The finite, positive feature scale in world-space units. Larger values produce broader features.</param>
        /// <param name="octaves">The positive number of noise layers.</param>
        /// <param name="persistence">The finite amplitude multiplier between layers, in [0, 1].</param>
        /// <param name="lacunarity">The finite, positive frequency multiplier between layers.</param>
        /// <param name="offset">The finite sample-space offset in world-space units.</param>
        /// <exception cref="ArgumentOutOfRangeException">A noise setting is outside its documented range.</exception>
        public PerlinNoisePartitionParameters(
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (!float.IsFinite(scale) || scale <= 0f)
                throw new ArgumentOutOfRangeException(nameof(scale));
            if (octaves <= 0)
                throw new ArgumentOutOfRangeException(nameof(octaves));
            if (!float.IsFinite(persistence) || persistence < 0f || persistence > 1f)
                throw new ArgumentOutOfRangeException(nameof(persistence));
            if (!float.IsFinite(lacunarity) || lacunarity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(lacunarity));
            if (!offset.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(offset));

            Scale = scale;
            Octaves = octaves;
            Persistence = persistence;
            Lacunarity = lacunarity;
            Offset = offset;
        }

        /// <summary>Gets the base feature scale in world-space units.</summary>
        public float Scale { get; }

        /// <summary>Gets the number of noise layers.</summary>
        public int Octaves { get; }

        /// <summary>Gets the amplitude multiplier between layers.</summary>
        public float Persistence { get; }

        /// <summary>Gets the frequency multiplier between layers.</summary>
        public float Lacunarity { get; }

        /// <summary>Gets the sample-space offset in world-space units.</summary>
        public VectorXY Offset { get; }
    }
}
