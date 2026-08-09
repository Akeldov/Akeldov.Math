using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents the contribution of an influence source at a sampled point.
    /// </summary>
    /// <typeparam name="TValue">The value type contributed by the source.</typeparam>
    public readonly struct InfluenceSample<TValue>
    {
        /// <summary>
        /// Initializes a new influence sample.
        /// </summary>
        /// <param name="value">The source value at the sampled point.</param>
        /// <param name="sourcePoint">The point on the influence source used for the sample.</param>
        /// <param name="distance">The distance from the requested point to <paramref name="sourcePoint"/>.</param>
        /// <param name="weight">The source weight at <paramref name="sourcePoint"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="distance"/> is negative, NaN, or infinite, or when
        /// <paramref name="weight"/> is negative or NaN.
        /// </exception>
        public InfluenceSample(TValue value, PointXY sourcePoint, float distance, float weight)
        {
            PointXYValidation.ThrowIfNotFinite(
                sourcePoint,
                nameof(sourcePoint),
                "Influence sample source point coordinates must be finite.");

            if (distance < 0f || float.IsNaN(distance) || float.IsInfinity(distance))
                throw new ArgumentOutOfRangeException(nameof(distance), "Influence sample distance must be finite and non-negative.");

            if (weight < 0f || float.IsNaN(weight))
                throw new ArgumentOutOfRangeException(nameof(weight), "Influence source weight must be non-negative and not NaN.");

            Value = value;
            SourcePoint = sourcePoint;
            Distance = distance;
            Weight = weight;
        }

        /// <summary>
        /// Gets the value contributed by the influence source.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Gets the point on the influence source used for this sample.
        /// </summary>
        public PointXY SourcePoint { get; }

        /// <summary>
        /// Gets the distance from the requested point to <see cref="SourcePoint"/>.
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Gets the source weight at <see cref="SourcePoint"/>.
        /// </summary>
        public float Weight { get; }
    }
}
