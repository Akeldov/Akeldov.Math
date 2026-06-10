using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a floating-point influence field sampled from point influence sources.
    /// </summary>
    /// <remarks>
    /// Values returned by <see cref="Sample"/> are clamped to the inclusive range
    /// from <see cref="Min"/> to <see cref="Max"/>. The configured sampler may compute
    /// an extrapolated value, but this field preserves the <see cref="IFloatField"/> range contract.
    /// </remarks>
    public class FloatPointInfluenceField : PointInfluenceField<FloatPointInfluenceSource, float>, IFloatField
    {
        private readonly float _min;
        private readonly float _max;
        private readonly IReadOnlyList<float> _distinctValues;

        /// <summary>
        /// Initializes a new floating-point influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSources">The influence points used by the field.</param>
        public FloatPointInfluenceField(
            IInfluenceSampler<FloatPointInfluenceSource, float> sampler,
            IReadOnlyList<FloatPointInfluenceSource> influenceSources)
            : base(sampler, influenceSources)
        {
            (_min, _max, _distinctValues) = GetRangeAndDistinctValues(InfluenceSources);
        }

        /// <summary>
        /// Initializes a new floating-point influence field with source culling.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence points.</param>
        /// <param name="influenceSources">The influence points used by the field.</param>
        /// <param name="influenceSourceCuller">The culler used to select a subset of points for each sampled point.</param>
        public FloatPointInfluenceField(
            IInfluenceSampler<FloatPointInfluenceSource, float> sampler,
            IReadOnlyList<FloatPointInfluenceSource> influenceSources,
            IInfluenceSourceCuller<FloatPointInfluenceSource> influenceSourceCuller)
            : base(sampler, influenceSources, influenceSourceCuller)
        {
            (_min, _max, _distinctValues) = GetRangeAndDistinctValues(InfluenceSources);
        }

        /// <inheritdoc/>
        public float Min => _min;

        /// <inheritdoc/>
        public float Max => _max;

        /// <summary>
        /// Gets the read-only distinct source values used to define this field range.
        /// </summary>
        public IReadOnlyList<float> DistinctValues => _distinctValues;

        /// <inheritdoc/>
        public override float Sample(PointXY point)
        {
            float value = base.Sample(point);
            if (float.IsNaN(value))
                throw new InvalidOperationException("Influence sampler returned an invalid field value. Value must not be NaN.");

            return value.Clamp(Min, Max);
        }

        private static (float Min, float Max, IReadOnlyList<float> DistinctValues) GetRangeAndDistinctValues(
            IReadOnlyList<FloatPointInfluenceSource> influenceSources)
        {
            float min = influenceSources[0].Value;
            float max = min;
            var distinctValues = new List<float>(influenceSources.Count);
            var distinctValuesHashSet = new HashSet<float>(influenceSources.Count);

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
