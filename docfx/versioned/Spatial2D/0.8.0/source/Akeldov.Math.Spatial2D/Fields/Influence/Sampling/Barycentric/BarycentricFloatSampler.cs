using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Samples floating-point values using barycentric interpolation over nearby influence samples.
    /// </summary>
    /// <remarks>
    /// This sampler is a mathematical interpolation strategy. When the sampled point lies outside
    /// the selected segment or triangle, it may extrapolate and return a value outside the range of
    /// the source values. Bounded fields clamp the sampler result to their public range.
    /// <para>
    /// Source count behavior:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>One source: returns that source value.</description>
    /// </item>
    /// <item>
    /// <description>Two sources: interpolates or extrapolates along the segment between them.</description>
    /// </item>
    /// <item>
    /// <description>
    /// Three sources: interpolates or extrapolates using barycentric coordinates of the triangle.
    /// If the triangle is degenerate, falls back to segment interpolation using the first two sources.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// More than three sources: considers up to <see cref="MaxCandidateSamples"/> nearest effective samples, returns the first
    /// triangle containing the sampled point, otherwise uses the triangle with the smallest
    /// outside-triangle penalty. If no non-degenerate triangle exists, falls back to segment
    /// interpolation using the two nearest samples.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TSource">The influence source type to sample from.</typeparam>
    public class BarycentricFloatSampler<TSource> : IInfluenceSampler<TSource, float>
        where TSource : IInfluenceSource<float>
    {
        /// <summary>
        /// The default maximum number of nearest effective samples considered when choosing an interpolation triangle.
        /// </summary>
        /// <remarks>
        /// The sampler checks combinations of three candidate samples, so triangle search cost grows cubically with this value.
        /// </remarks>
        public const int DefaultMaxCandidateSamples = BarycentricSamplerDefaults.DefaultMaxCandidateSamples;

        private const float Epsilon = GeometryConstants.GeometryEpsilon;
        private const float WeightEpsilon = GeometryConstants.GeometryEpsilon;

        /// <summary>
        /// Gets the maximum number of nearest effective samples considered when choosing an interpolation triangle.
        /// </summary>
        public int MaxCandidateSamples { get; }

        /// <summary>
        /// Initializes a new barycentric floating-point influence sampler using <see cref="DefaultMaxCandidateSamples"/>.
        /// </summary>
        public BarycentricFloatSampler()
            : this(DefaultMaxCandidateSamples)
        { }

        /// <summary>
        /// Initializes a new barycentric floating-point influence sampler.
        /// </summary>
        /// <param name="maxCandidateSamples">The maximum number of nearest effective samples considered when choosing an interpolation triangle.</param>
        public BarycentricFloatSampler(int maxCandidateSamples)
        {
            if (maxCandidateSamples < BarycentricSamplerDefaults.MinCandidateSamples)
                throw new ArgumentOutOfRangeException(
                    nameof(maxCandidateSamples),
                    maxCandidateSamples,
                    "Candidate sample count must be at least three.");

            MaxCandidateSamples = maxCandidateSamples;
        }

        /// <summary>
        /// Samples a floating-point value at the specified point.
        /// </summary>
        /// <param name="sources">The influence sources used for interpolation. Must be non-null, non-empty, and contain no null elements.</param>
        /// <param name="point">The point to sample.</param>
        /// <returns>The interpolated or extrapolated floating-point value.</returns>
        public float Sample(IReadOnlyList<TSource> sources, PointXY point)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            int n = sources.Count;
            if (n <= 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(sources));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var sourceA = sources[0];
            if (sourceA is null)
                throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

            var sampleA = sourceA.GetInfluence(point);
            if (n == 1) return sampleA.Value;

            var sourceB = sources[1];
            if (sourceB is null)
                throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

            var sampleB = sourceB.GetInfluence(point);
            if (n == 2) return LerpOnSegment(sampleA, sampleB, point);

            var sourceC = sources[2];
            if (sourceC is null)
                throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

            var sampleC = sourceC.GetInfluence(point);
            if (n == 3)
                return InterpolateTriangle(sampleA, sampleB, sampleC, point);

            int k = System.Math.Min(MaxCandidateSamples, n);
            var nearest = GetNearestSamples(sources, point, k, sampleA, sampleB, sampleC);

            for (int i = 0; i < k; i++)
                for (int j = i + 1; j < k; j++)
                    for (int m = j + 1; m < k; m++)
                    {
                        var a = nearest[i];
                        var b = nearest[j];
                        var c = nearest[m];

                        if (!TryBarycentric(a.SourcePoint, b.SourcePoint, c.SourcePoint, point,
                            out float lA, out float lB, out float lC))
                            continue;

                        if (lA >= -GeometryConstants.GeometryEpsilon &&
                            lB >= -GeometryConstants.GeometryEpsilon &&
                            lC >= -GeometryConstants.GeometryEpsilon)
                            return a.Value * lA +
                                   b.Value * lB +
                                   c.Value * lC;
                    }

            float bestPenalty = float.PositiveInfinity;
            var bestA = nearest[0];
            var bestB = nearest[1];
            var bestC = nearest[2];
            float bestLA = 0, bestLB = 0, bestLC = 0;

            for (int i = 0; i < k; i++)
                for (int j = i + 1; j < k; j++)
                    for (int m = j + 1; m < k; m++)
                    {
                        var a = nearest[i];
                        var b = nearest[j];
                        var c = nearest[m];

                        var pa = a.SourcePoint;
                        var pb = b.SourcePoint;
                        var pc = c.SourcePoint;

                        if (!TryBarycentric(pa, pb, pc, point,
                            out float lA, out float lB, out float lC))
                            continue;

                        float penalty =
                            MathF.Max(0f, -lA) +
                            MathF.Max(0f, -lB) +
                            MathF.Max(0f, -lC);

                        float area2 = MathF.Abs(Cross(pb - pa, pc - pa));
                        if (area2 <= GeometryConstants.GeometryEpsilon)
                            continue;

                        if (penalty < bestPenalty)
                        {
                            bestPenalty = penalty;
                            bestA = a;
                            bestB = b;
                            bestC = c;
                            bestLA = lA;
                            bestLB = lB;
                            bestLC = lC;
                        }
                    }

            if (float.IsPositiveInfinity(bestPenalty))
                return LerpOnSegment(nearest[0], nearest[1], point);

            return bestA.Value * bestLA +
                   bestB.Value * bestLB +
                   bestC.Value * bestLC;
        }

        private static float InterpolateTriangle(
            InfluenceSample<float> a,
            InfluenceSample<float> b,
            InfluenceSample<float> c,
            PointXY p)
        {
            if (!TryBarycentric(a.SourcePoint, b.SourcePoint, c.SourcePoint, p,
                out float lA, out float lB, out float lC))
            {
                return LerpOnSegment(a, b, p);
            }

            return a.Value * lA +
                   b.Value * lB +
                   c.Value * lC;
        }

        private static bool TryBarycentric(
            PointXY a, PointXY b, PointXY c, PointXY p,
            out float lA, out float lB, out float lC)
        {
            float denom = Cross(b - a, c - a);
            if (denom.IsAlmostZero(Epsilon))
            {
                lA = lB = lC = 0f;
                return false;
            }

            lB = Cross(p - a, c - a) / denom;
            lC = Cross(b - a, p - a) / denom;
            lA = 1f - lB - lC;
            return true;
        }

        private static float LerpOnSegment(
            InfluenceSample<float> a,
            InfluenceSample<float> b,
            PointXY p)
        {
            var pa = a.SourcePoint;
            var pb = b.SourcePoint;

            var ab = pb - pa;
            var ap = p - pa;

            float ab2 = ab.X * ab.X + ab.Y * ab.Y;
            if (ab2 <= GeometryConstants.GeometryEpsilonSquared)
                return a.Value;

            float t = (ap.X * ab.X + ap.Y * ab.Y) / ab2;

            return a.Value * (1f - t) +
                   b.Value * t;
        }

        private static float EffectiveDistance(InfluenceSample<float> sample)
        {
            float weight = MathF.Max(sample.Weight, WeightEpsilon);
            return sample.Distance / weight;
        }

        private static InfluenceSample<float>[] GetNearestSamples(
            IReadOnlyList<TSource> sources,
            PointXY point,
            int count,
            InfluenceSample<float> sampleA,
            InfluenceSample<float> sampleB,
            InfluenceSample<float> sampleC)
        {
            var nearest = new InfluenceSample<float>[count];
            var effectiveDistances = new float[count];

            InsertNearest(nearest, effectiveDistances, 0, sampleA);
            InsertNearest(nearest, effectiveDistances, 1, sampleB);
            InsertNearest(nearest, effectiveDistances, 2, sampleC);

            for (int i = 3; i < sources.Count; i++)
            {
                var source = sources[i];
                if (source is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

                var sample = source.GetInfluence(point);
                float effectiveDistance = EffectiveDistance(sample);

                if (i < count || effectiveDistance < effectiveDistances[count - 1])
                    InsertNearest(nearest, effectiveDistances, System.Math.Min(i, count), sample, effectiveDistance);
            }

            return nearest;
        }

        private static void InsertNearest(
            InfluenceSample<float>[] nearest,
            float[] effectiveDistances,
            int existingCount,
            InfluenceSample<float> sample)
        {
            InsertNearest(nearest, effectiveDistances, existingCount, sample, EffectiveDistance(sample));
        }

        private static void InsertNearest(
            InfluenceSample<float>[] nearest,
            float[] effectiveDistances,
            int existingCount,
            InfluenceSample<float> sample,
            float effectiveDistance)
        {
            int index = System.Math.Min(existingCount, nearest.Length - 1);

            while (index > 0 && effectiveDistance < effectiveDistances[index - 1])
            {
                nearest[index] = nearest[index - 1];
                effectiveDistances[index] = effectiveDistances[index - 1];
                index--;
            }

            nearest[index] = sample;
            effectiveDistances[index] = effectiveDistance;
        }

        private static float Cross(VectorXY u, VectorXY v)
            => u.X * v.Y - u.Y * v.X;
    }
}
