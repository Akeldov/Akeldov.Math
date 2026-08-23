using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents an integer influence field sampled from point influence sources.
    /// </summary>
    /// <remarks>
    /// Values returned by <see cref="Sample"/> are clamped to the inclusive range
    /// from <see cref="Min"/> to <see cref="Max"/>. The configured sampler may compute
    /// an extrapolated value, but this field preserves the <see cref="IIntField"/> range contract.
    /// </remarks>
    public class IntPointInfluenceField : PointInfluenceField<IntPointInfluenceSource, int>, IIntField
    {
        private readonly int _min;
        private readonly int _max;
        private readonly IReadOnlyList<int> _distinctValues;

        /// <summary>
        /// Initializes a new integer influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSources">The influence points used by the field.</param>
        public IntPointInfluenceField(
            IInfluenceSampler<IntPointInfluenceSource, int> sampler,
            IReadOnlyList<IntPointInfluenceSource> influenceSources)
            : base(sampler, influenceSources)
        {
            (_min, _max, _distinctValues) = GetRangeAndDistinctValues(InfluenceSources);
        }

        /// <summary>
        /// Initializes a new integer influence field backed by a source index.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSourceIndex">The index that owns and selects the influence points.</param>
        public IntPointInfluenceField(
            IInfluenceSampler<IntPointInfluenceSource, int> sampler,
            IInfluenceSourceIndex<IntPointInfluenceSource> influenceSourceIndex)
            : base(sampler, influenceSourceIndex)
        {
            (_min, _max, _distinctValues) = GetRangeAndDistinctValues(InfluenceSources);
        }

        /// <inheritdoc/>
        public int Min => _min;

        /// <inheritdoc/>
        public int Max => _max;

        /// <summary>
        /// Gets the read-only distinct source values used to define this field range.
        /// </summary>
        public IReadOnlyList<int> DistinctValues => _distinctValues;

        /// <inheritdoc/>
        public override int Sample(PointXY point)
        {
            int value = base.Sample(point);

            if (value < Min)
                return Min;

            if (value > Max)
                return Max;

            return value;
        }

        private static (int Min, int Max, IReadOnlyList<int> DistinctValues) GetRangeAndDistinctValues(
            IReadOnlyList<IntPointInfluenceSource> influenceSources)
        {
            int min = influenceSources[0].Value;
            int max = min;
            var distinctValues = new List<int>(influenceSources.Count);
            var distinctValuesHashSet = new HashSet<int>(influenceSources.Count);

            for (int i = 0; i < influenceSources.Count; i++)
            {
                var value = influenceSources[i].Value;

                if (value < min)
                    min = value;

                if (value > max)
                    max = value;

                if (!distinctValuesHashSet.Contains(value))
                {
                    distinctValuesHashSet.Add(value);
                    distinctValues.Add(value);
                }
            }

            return (min, max, distinctValues.AsReadOnly());
        }
    }
}
