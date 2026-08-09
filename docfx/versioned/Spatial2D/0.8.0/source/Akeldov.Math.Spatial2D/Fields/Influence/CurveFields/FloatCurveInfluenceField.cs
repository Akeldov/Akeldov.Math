using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a floating-point influence field sampled from curve influence sources.
    /// </summary>
    /// <remarks>
    /// Values returned by <see cref="Sample"/> are clamped to the inclusive range
    /// from <see cref="Min"/> to <see cref="Max"/>. The configured sampler may compute
    /// an extrapolated value, but this field preserves the <see cref="IFloatField"/> range contract.
    /// </remarks>
    public class FloatCurveInfluenceField : CurveInfluenceField<ICurveInfluenceSource<float>, float>, IFloatField
    {
        private readonly float _min;
        private readonly float _max;

        /// <summary>
        /// Initializes a new floating-point curve influence field.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence curves.</param>
        /// <param name="influenceSources">The influence curves used by the field.</param>
        /// <param name="min">The minimum value returned by the field.</param>
        /// <param name="max">The maximum value returned by the field.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="min"/> or <paramref name="max"/> is NaN, or when
        /// <paramref name="min"/> is greater than <paramref name="max"/>.
        /// </exception>
        public FloatCurveInfluenceField(
            IInfluenceSampler<ICurveInfluenceSource<float>, float> sampler,
            IReadOnlyList<ICurveInfluenceSource<float>> influenceSources,
            float min,
            float max)
            : base(sampler, influenceSources)
        {
            ValidateRange(min, max);

            _min = min;
            _max = max;
        }

        /// <summary>
        /// Initializes a new floating-point curve influence field with source culling.
        /// </summary>
        /// <param name="sampler">The strategy used to combine influence curves.</param>
        /// <param name="influenceSources">The influence curves used by the field.</param>
        /// <param name="min">The minimum value returned by the field.</param>
        /// <param name="max">The maximum value returned by the field.</param>
        /// <param name="influenceSourceCuller">The culler used to select a subset of curves for each sampled point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="min"/> or <paramref name="max"/> is NaN, or when
        /// <paramref name="min"/> is greater than <paramref name="max"/>.
        /// </exception>
        public FloatCurveInfluenceField(
            IInfluenceSampler<ICurveInfluenceSource<float>, float> sampler,
            IReadOnlyList<ICurveInfluenceSource<float>> influenceSources,
            float min,
            float max,
            IInfluenceSourceCuller<ICurveInfluenceSource<float>> influenceSourceCuller)
            : base(sampler, influenceSources, influenceSourceCuller)
        {
            ValidateRange(min, max);

            _min = min;
            _max = max;
        }

        /// <inheritdoc/>
        public float Min => _min;

        /// <inheritdoc/>
        public float Max => _max;

        /// <inheritdoc/>
        public override float Sample(PointXY point)
        {
            float value = base.Sample(point);
            if (float.IsNaN(value))
                throw new InvalidOperationException("Influence sampler returned an invalid field value. Value must not be NaN.");

            return value.Clamp(Min, Max);
        }

        private static void ValidateRange(float min, float max)
        {
            if (float.IsNaN(min))
                throw new ArgumentOutOfRangeException(nameof(min), "Curve influence field minimum must not be NaN.");

            if (float.IsNaN(max))
                throw new ArgumentOutOfRangeException(nameof(max), "Curve influence field maximum must not be NaN.");

            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "Curve influence field minimum must be less than or equal to maximum.");
        }
    }
}
