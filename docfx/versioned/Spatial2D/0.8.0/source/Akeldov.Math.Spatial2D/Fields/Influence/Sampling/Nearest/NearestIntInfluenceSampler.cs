using System.Collections.Generic;
using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Samples the integer value contributed by the nearest influence source.
    /// </summary>
    /// <typeparam name="TSource">The influence source type to sample from.</typeparam>
    public class NearestIntInfluenceSampler<TSource> : IInfluenceSampler<TSource, int>
        where TSource : IInfluenceSource<int>
    {
        /// <summary>
        /// Initializes a new nearest-source integer influence sampler.
        /// </summary>
        public NearestIntInfluenceSampler()
        { }

        /// <summary>
        /// Samples the value of the nearest influence source at the specified point.
        /// </summary>
        /// <param name="sources">The influence sources to search. Must be non-null, non-empty, and contain no null elements.</param>
        /// <param name="point">The point to sample.</param>
        /// <returns>The integer value contributed by the nearest source.</returns>
        public int Sample(IReadOnlyList<TSource> sources, PointXY point)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(sources));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var nearestSource = sources[0];
            if (nearestSource is null)
                throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

            var nearestInfluence = nearestSource.GetInfluence(point);

            for (var i = 1; i < sources.Count; i++)
            {
                var candidateSource = sources[i];
                if (candidateSource is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

                var candidateInfluence = candidateSource.GetInfluence(point);

                if (candidateInfluence.Distance < nearestInfluence.Distance)
                    nearestInfluence = candidateInfluence;
            }

            return nearestInfluence.Value;
        }
    }
}
