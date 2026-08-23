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
        private readonly IInfluenceSourceIndex<TSource>? _influenceSourceIndex;
        private readonly IReadOnlyList<TSource> _influenceSources;

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
            _influenceSourceIndex = null;
            _influenceSources = Array.AsReadOnly(CopyInfluenceSources(influenceSources));
        }

        /// <summary>
        /// Initializes a new influence field that selects sources through the specified index.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence sources.</param>
        /// <param name="influenceSourceIndex">
        /// The index that owns the immutable source snapshot and selects sources for each sampled point.
        /// </param>
        public InfluenceField(
            IInfluenceSampler<TSource, TValue> sampler,
            IInfluenceSourceIndex<TSource> influenceSourceIndex)
        {
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            _influenceSourceIndex = influenceSourceIndex ?? throw new ArgumentNullException(nameof(influenceSourceIndex));
            _influenceSources = influenceSourceIndex.Sources;

            if (_influenceSources == null)
                throw new ArgumentException("Influence source index must expose a non-null source snapshot.", nameof(influenceSourceIndex));

            if (_influenceSources.Count == 0)
                throw new ArgumentException("Influence source index must contain at least one source.", nameof(influenceSourceIndex));

            for (int i = 0; i < _influenceSources.Count; i++)
            {
                if (_influenceSources[i] is null)
                    throw new ArgumentException("Influence source index snapshot cannot contain null elements.", nameof(influenceSourceIndex));
            }
        }

        /// <summary>
        /// Gets the immutable source snapshot used by this field. In indexed mode this is the
        /// snapshot owned and exposed by the configured index; otherwise it is a field-owned copy.
        /// </summary>
        public IReadOnlyList<TSource> InfluenceSources => _influenceSources;

        /// <summary>
        /// Samples the field value at the specified point by delegating to the configured sampler.
        /// </summary>
        /// <param name="point">The point to sample.</param>
        /// <returns>
        /// The sampler result. Derived bounded field types may clamp this value to their public range.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The configured index returned a null or empty source list, or a list containing null.
        /// </exception>
        public virtual TValue Sample(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            if (_influenceSourceIndex == null)
                return _sampler.Sample(_influenceSources, point);

            List<TSource> selectedSources = _influenceSourceIndex.SelectSources(point);

            if (selectedSources == null)
                throw new InvalidOperationException(
                    "Influence source index returned null. Index implementations must return a non-empty source list.");

            if (selectedSources.Count == 0)
                throw new InvalidOperationException(
                    "Influence source index returned an empty source list. Index implementations must select at least one source.");

            for (int i = 0; i < selectedSources.Count; i++)
            {
                if (selectedSources[i] is null)
                    throw new InvalidOperationException(
                        "Influence source index returned a source list containing null.");
            }

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
