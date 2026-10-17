using System;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Defines a territory identifier and its desired relative area in a Perlin-noise partition.</summary>
    public sealed class PerlinNoisePartitionTarget
    {
        /// <summary>Initializes an immutable territory target.</summary>
        /// <param name="id">The non-negative identifier, unique within the partition.</param>
        /// <param name="areaShare">
        /// The finite, non-negative relative area share. Shares are normalized by their sum:
        /// 0.2, 0.3, 0.5 and 20, 30, 50 describe the same proportions. Zero requests an empty territory.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">The identifier is negative or the share is negative or non-finite.</exception>
        public PerlinNoisePartitionTarget(int id, double areaShare)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (!double.IsFinite(areaShare) || areaShare < 0d)
                throw new ArgumentOutOfRangeException(nameof(areaShare));

            Id = id;
            AreaShare = areaShare;
        }

        /// <summary>Gets the non-negative territory identifier.</summary>
        public int Id { get; }

        /// <summary>Gets the desired relative area share, normalized against the other targets during partitioning.</summary>
        public double AreaShare { get; }
    }
}
