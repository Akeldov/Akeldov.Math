using System;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    /// <summary>
    /// Represents a point produced by Poisson disk sampling.
    /// </summary>
    public readonly struct PoissonDiskPointSample
    {
        /// <summary>
        /// Initializes a new Poisson disk point sample.
        /// </summary>
        /// <param name="point">The sampled point.</param>
        /// <param name="minimalDistance">The finite positive minimal distance associated with the sampled point.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minimalDistance"/> is not finite and positive.</exception>
        public PoissonDiskPointSample(PointXY point, float minimalDistance)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Sample point coordinates must be finite.");

            if (minimalDistance <= 0f || float.IsNaN(minimalDistance) || float.IsInfinity(minimalDistance))
                throw new ArgumentOutOfRangeException(nameof(minimalDistance), "Minimal distance must be finite and positive.");

            Point = point;
            MinimalDistance = minimalDistance;
        }

        /// <summary>
        /// Gets the sampled point.
        /// </summary>
        public PointXY Point { get; }

        /// <summary>
        /// Gets the minimal distance associated with the sampled point.
        /// </summary>
        public float MinimalDistance { get; }

        /// <summary>
        /// Deconstructs the sample into its point and minimal distance.
        /// </summary>
        /// <param name="point">The sampled point.</param>
        /// <param name="minimalDistance">The minimal distance associated with the sampled point.</param>
        public void Deconstruct(out PointXY point, out float minimalDistance)
        {
            point = Point;
            minimalDistance = MinimalDistance;
        }
    }
}
