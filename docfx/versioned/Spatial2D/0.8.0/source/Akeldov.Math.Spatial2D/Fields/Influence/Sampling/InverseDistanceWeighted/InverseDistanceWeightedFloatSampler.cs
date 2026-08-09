using System.Collections.Generic;
using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Samples floating-point values using inverse-distance weighting across influence sources.
    /// </summary>
    /// <typeparam name="TSource">The influence source type to sample from.</typeparam>
    public class InverseDistanceWeightedFloatSampler<TSource> : IInfluenceSampler<TSource, float>
        where TSource : IInfluenceSource<float>
    {
        /// <summary>
        /// Initializes a new inverse-distance weighted floating-point sampler.
        /// </summary>
        public InverseDistanceWeightedFloatSampler()
        { }

        /// <summary>
        /// Samples a floating-point value at the specified point.
        /// </summary>
        /// <param name="sources">The influence sources used for weighting. Must be non-null, non-empty, and contain no null elements.</param>
        /// <param name="point">The point to sample.</param>
        /// <returns>The inverse-distance weighted value.</returns>
        public float Sample(IReadOnlyList<TSource> sources, PointXY point)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(sources));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float totalWeight = 0f;
            float weightedSum = 0f;

            for (int i = 0; i < sources.Count; i++)
            {
                var source = sources[i];
                if (source is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

                var influence = source.GetInfluence(point);
                if (influence.Weight <= 0f || float.IsNaN(influence.Weight) || float.IsInfinity(influence.Weight))
                    throw new InvalidOperationException("Inverse-distance weighted sampler requires finite positive source weight.");

                if (influence.Distance <= GeometryConstants.GeometryEpsilon)
                    return influence.Value;

                float weight = influence.Weight / influence.Distance;
                totalWeight += weight;
                weightedSum += influence.Value * weight;
            }

            return totalWeight > 0f ? (weightedSum / totalWeight) : 0f;
        }
    }
}
