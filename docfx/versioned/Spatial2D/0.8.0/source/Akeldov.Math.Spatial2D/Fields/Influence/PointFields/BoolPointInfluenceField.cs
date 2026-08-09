using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a Boolean influence field sampled from point influence sources.
    /// </summary>
    public class BoolPointInfluenceField : PointInfluenceField<BoolPointInfluenceSource, bool>
    {
        private readonly IReadOnlyList<bool> _distinctValues;

        /// <summary>
        /// Initializes a new Boolean influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSources">The influence points used by the field.</param>
        public BoolPointInfluenceField(
            IInfluenceSampler<BoolPointInfluenceSource, bool> sampler,
            IReadOnlyList<BoolPointInfluenceSource> influenceSources)
            : base(sampler, influenceSources)
        {
            _distinctValues = GetDistinctValues(InfluenceSources);
        }

        /// <summary>
        /// Initializes a new Boolean influence field with source culling.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSources">The influence points used by the field.</param>
        /// <param name="influenceSourceCuller">The culler used to select a subset of points for each sampled point.</param>
        public BoolPointInfluenceField(
            IInfluenceSampler<BoolPointInfluenceSource, bool> sampler,
            IReadOnlyList<BoolPointInfluenceSource> influenceSources,
            IInfluenceSourceCuller<BoolPointInfluenceSource> influenceSourceCuller)
            : base(sampler, influenceSources, influenceSourceCuller)
        {
            _distinctValues = GetDistinctValues(InfluenceSources);
        }

        /// <summary>
        /// Gets the read-only distinct source values in first-occurrence order.
        /// </summary>
        public IReadOnlyList<bool> DistinctValues => _distinctValues;

        private static IReadOnlyList<bool> GetDistinctValues(
            IReadOnlyList<BoolPointInfluenceSource> influenceSources)
        {
            bool firstValue = influenceSources[0].Value;
            var distinctValues = new List<bool>(2) { firstValue };

            for (int i = 1; i < influenceSources.Count; i++)
            {
                if (influenceSources[i].Value != firstValue)
                {
                    distinctValues.Add(influenceSources[i].Value);
                    break;
                }
            }

            return distinctValues.AsReadOnly();
        }
    }
}
