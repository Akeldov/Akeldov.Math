using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a field whose value is sampled from a collection of influence sources.
    /// </summary>
    /// <typeparam name="TSource">The influence source type.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public class InfluenceField<TSource, TValue> : IField<TValue>
        where TSource : IInfluenceSource<TValue>
    {
        private readonly IInfluenceSampler<TSource, TValue> _sampler;
        private readonly IInfluenceSourceCuller<TSource>? _influenceSourceCuller;
        private readonly TSource[] _influenceSources;
        private readonly IReadOnlyList<TSource> _readOnlyInfluenceSources;

        /// <summary>
        /// Initializes a new influence field with the specified sampler and influence sources.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence sources.</param>
        /// <param name="influenceSources">The influence sources used by the field.</param>
        public InfluenceField(
            IInfluenceSampler<TSource, TValue> sampler,
            IReadOnlyList<TSource> influenceSources)
        {
            if (influenceSources == null)
                throw new ArgumentNullException(nameof(influenceSources));

            if (influenceSources.Count == 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(influenceSources));

            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            _influenceSourceCuller = null;
            _influenceSources = CopyInfluenceSources(influenceSources);
            _readOnlyInfluenceSources = Array.AsReadOnly(_influenceSources);
        }

        /// <summary>
        /// Initializes a new influence field with source culling.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence sources.</param>
        /// <param name="influenceSources">The influence sources used by the field.</param>
        /// <param name="influenceSourceCuller">
        /// The culler used to select a non-empty subset of sources for each sampled point.
        /// </param>
        public InfluenceField(
            IInfluenceSampler<TSource, TValue> sampler,
            IReadOnlyList<TSource> influenceSources,
            IInfluenceSourceCuller<TSource> influenceSourceCuller)
        {
            if (influenceSources == null)
                throw new ArgumentNullException(nameof(influenceSources));

            if (influenceSources.Count == 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(influenceSources));

            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            _influenceSourceCuller = influenceSourceCuller ?? throw new ArgumentNullException(nameof(influenceSourceCuller));
            _influenceSources = CopyInfluenceSources(influenceSources);
            _readOnlyInfluenceSources = Array.AsReadOnly(_influenceSources);
        }

        /// <summary>
        /// Gets the read-only influence sources configured for this field.
        /// </summary>
        public IReadOnlyList<TSource> InfluenceSources => _readOnlyInfluenceSources;

        /// <summary>
        /// Samples the field value at the specified point by delegating to the configured sampler.
        /// </summary>
        /// <param name="point">The point to sample.</param>
        /// <returns>
        /// The sampler result. Derived bounded field types may clamp this value to their public range.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The configured culler returned a null or empty source list.
        /// </exception>
        public virtual TValue Sample(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            if (_influenceSourceCuller == null)
                return _sampler.Sample(_influenceSources, point);

            IReadOnlyList<TSource> selectedSources = _influenceSourceCuller.Cull(point);

            if (selectedSources == null)
                throw new InvalidOperationException(
                    "Influence source culler returned null. Culler implementations must return a non-empty source list.");

            if (selectedSources.Count == 0)
                throw new InvalidOperationException(
                    "Influence source culler returned an empty source list. Culler implementations must return at least one source.");

            return _sampler.Sample(selectedSources, point);
        }

        private static TSource[] CopyInfluenceSources(IReadOnlyList<TSource> influenceSources)
        {
            var copy = new TSource[influenceSources.Count];
            for (int i = 0; i < influenceSources.Count; i++)
            {
                var source = influenceSources[i];
                if (source is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(influenceSources));

                copy[i] = source;
            }

            return copy;
        }
    }
}
