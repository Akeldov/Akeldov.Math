using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a field whose value is sampled from point influence sources.
    /// </summary>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public class PointInfluenceField<TPointSource, TValue> : InfluenceField<TPointSource, TValue>
        where TPointSource : IPointInfluenceSource<TValue>
    {
        /// <summary>
        /// Initializes a new point influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine point influence sources.</param>
        /// <param name="influenceSources">The point influence sources used by the field.</param>
        public PointInfluenceField(
            IInfluenceSampler<TPointSource, TValue> sampler,
            IReadOnlyList<TPointSource> influenceSources)
            : base(sampler, influenceSources)
        {
        }

        /// <summary>
        /// Initializes a new point influence field backed by a source index.
        /// </summary>
        /// <param name="sampler">The strategy used to combine point influence sources.</param>
        /// <param name="influenceSourceIndex">The index that owns and selects the point influence sources.</param>
        public PointInfluenceField(
            IInfluenceSampler<TPointSource, TValue> sampler,
            IInfluenceSourceIndex<TPointSource> influenceSourceIndex)
            : base(sampler, influenceSourceIndex)
        {
        }
    }
}
