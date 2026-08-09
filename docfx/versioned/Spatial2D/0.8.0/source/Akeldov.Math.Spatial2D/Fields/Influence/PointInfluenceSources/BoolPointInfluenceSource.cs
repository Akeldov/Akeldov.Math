using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a point influence source that contributes a Boolean value.
    /// </summary>
    public class BoolPointInfluenceSource : IPointInfluenceSource<bool>
    {
        /// <summary>
        /// Initializes a new Boolean influence source.
        /// </summary>
        /// <param name="weight">The source weight used by influence samplers.</param>
        /// <param name="position">The source position.</param>
        /// <param name="value">The value contributed by this source.</param>
        public BoolPointInfluenceSource(float weight, PointXY position, bool value)
        {
            PointXYValidation.ThrowIfNotFinite(
                position,
                nameof(position),
                "Influence source position coordinates must be finite.");

            if (weight < 0f || float.IsNaN(weight))
                throw new ArgumentOutOfRangeException(nameof(weight), "Influence source weight must be non-negative and not NaN.");

            Weight = weight;
            Position = position;
            Value = value;
        }

        /// <summary>
        /// Gets the source weight used by influence samplers.
        /// </summary>
        public float Weight { get; }

        /// <summary>
        /// Gets the source position.
        /// </summary>
        public PointXY Position { get; }

        /// <summary>
        /// Gets the value contributed by this source.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        /// Returns the distance from this source to the specified point.
        /// </summary>
        /// <param name="point">The point to measure to.</param>
        /// <returns>The Euclidean distance from the source position to the point.</returns>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return Position.Distance(point);
        }

        /// <summary>
        /// Gets the influence contribution of this source for the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>The value, source point, distance, and weight used by influence samplers.</returns>
        public InfluenceSample<bool> GetInfluence(PointXY point)
        {
            return new InfluenceSample<bool>(Value, Position, Distance(point), Weight);
        }
    }
}
