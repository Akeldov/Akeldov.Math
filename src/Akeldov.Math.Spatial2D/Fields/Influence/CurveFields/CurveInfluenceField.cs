using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents an influence field whose sources are projectable curves.
    /// </summary>
    /// <typeparam name="TCurveSource">The curve influence source type.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public class CurveInfluenceField<TCurveSource, TValue> : InfluenceField<TCurveSource, TValue>
        where TCurveSource : ICurveInfluenceSource<TValue>
    {
        /// <summary>
        /// Initializes a new curve influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence curves.</param>
        /// <param name="influenceSources">The influence curves used by the field.</param>
        public CurveInfluenceField(
            IInfluenceSampler<TCurveSource, TValue> sampler,
            IReadOnlyList<TCurveSource> influenceSources)
            : base(sampler, influenceSources)
        {
        }

        /// <summary>
        /// Initializes a new curve influence field backed by a source index.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence curves.</param>
        /// <param name="influenceSourceIndex">The index that owns and selects the influence curves.</param>
        public CurveInfluenceField(
            IInfluenceSampler<TCurveSource, TValue> sampler,
            IInfluenceSourceIndex<TCurveSource> influenceSourceIndex)
            : base(sampler, influenceSourceIndex)
        {
        }
    }
}
