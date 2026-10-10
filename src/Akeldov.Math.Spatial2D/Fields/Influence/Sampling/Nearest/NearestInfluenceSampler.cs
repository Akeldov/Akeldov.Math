using System.Collections.Generic;
using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Akeldov.Math.Spatial2D.Fields
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
#pragma warning disable IDE0270 // Use coalesce expression
    /// <summary>
    /// Samples the value contributed by the nearest influence source.
    /// </summary>
    /// <typeparam name="TSource">The influence source type to sample from.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public class NearestInfluenceSampler<TSource, TValue> : IInfluenceSampler<TSource, TValue>
        where TSource : IInfluenceSource<TValue>
    {
        /// <summary>
        /// Initializes a new nearest-source influence sampler.
        /// </summary>
        public NearestInfluenceSampler()
        { }

        /// <summary>
        /// Samples the value of the nearest influence source at the specified point.
        /// </summary>
        /// <param name="sources">The influence sources to search. Must be non-null, non-empty, and contain no null elements.</param>
        /// <param name="point">The point to sample.</param>
        /// <returns>The value contributed by the nearest source.</returns>
        public TValue Sample(IReadOnlyList<TSource> sources, PointXY point)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(sources));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            TSource nearestSource = sources[0];
            if (nearestSource is null)
                throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

            InfluenceSample<TValue> nearestInfluence = nearestSource.GetInfluence(point);

            for (int i = 1; i < sources.Count; i++)
            {
                TSource candidateSource = sources[i];
                if (candidateSource is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

                InfluenceSample<TValue> candidateInfluence = candidateSource.GetInfluence(point);

                if (candidateInfluence.Distance < nearestInfluence.Distance)
                    nearestInfluence = candidateInfluence;
            }

            return nearestInfluence.Value;
        }
    }
#pragma warning restore IDE0270 // Use coalesce expression
}
