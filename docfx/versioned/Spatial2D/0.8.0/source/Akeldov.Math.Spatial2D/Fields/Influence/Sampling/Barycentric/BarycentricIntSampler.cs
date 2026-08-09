using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Samples integer values using barycentric interpolation over nearby influence samples.
    /// </summary>
    /// <remarks>
    /// This sampler is a mathematical interpolation strategy. When the sampled point lies outside
    /// the selected segment or triangle, it may extrapolate and return a value outside the range of
    /// the source values. Bounded fields clamp the sampler result to their public range.
    /// </remarks>
    /// <typeparam name="TSource">The influence source type to sample from.</typeparam>
    public class BarycentricIntSampler<TSource> : IInfluenceSampler<TSource, int>
        where TSource : IInfluenceSource<int>
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
        /// Initializes a new barycentric integer influence sampler using <see cref="DefaultMaxCandidateSamples"/>.
        /// </summary>
        public BarycentricIntSampler()
            : this(DefaultMaxCandidateSamples)
        { }

        /// <summary>
        /// Initializes a new barycentric integer influence sampler.
        /// </summary>
        /// <param name="maxCandidateSamples">The maximum number of nearest effective samples considered when choosing an interpolation triangle.</param>
        public BarycentricIntSampler(int maxCandidateSamples)
        {
            if (maxCandidateSamples < BarycentricSamplerDefaults.MinCandidateSamples)
                throw new ArgumentOutOfRangeException(
                    nameof(maxCandidateSamples),
                    maxCandidateSamples,
                    "Candidate sample count must be at least three.");

            MaxCandidateSamples = maxCandidateSamples;
        }

        /// <summary>
        /// Samples an integer value at the specified point.
        /// </summary>
        /// <param name="sources">The influence sources used for interpolation. Must be non-null, non-empty, and contain no null elements.</param>
        /// <param name="point">The point to sample.</param>
        /// <returns>The interpolated or extrapolated value, rounded to the nearest integer.</returns>
        public int Sample(IReadOnlyList<TSource> sources, PointXY point)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            int n = sources.Count;
            if (n <= 0)
                throw new ArgumentException("Influence sources collection must not be empty.", nameof(sources));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var samples = new InfluenceSample<int>[n];
            for (var i = 0; i < n; i++)
            {
                var source = sources[i];
                if (source is null)
                    throw new ArgumentException("Influence sources collection cannot contain null elements.", nameof(sources));

                samples[i] = source.GetInfluence(point);
            }

            if (n == 1) return samples[0].Value;
            if (n == 2) return (int)MathF.Round(LerpOnSegment(samples[0], samples[1], point));

            if (n == 3)
                return (int)MathF.Round(InterpolateTriangle(samples[0], samples[1], samples[2], point));

            int k = System.Math.Min(MaxCandidateSamples, n);
            var nearest = GetNearestSamples(samples, k);

            for (int i = 0; i < k; i++)
                for (int j = i + 1; j < k; j++)
                    for (int m = j + 1; m < k; m++)
                    {
                        var a = nearest[i];
                        var b = nearest[j];
                        var c = nearest[m];

                        if (!TryBarycentric(a.SourcePoint, b.SourcePoint, c.SourcePoint, point, out float lA, out float lB, out float lC))
                            continue;

                        if (lA >= -GeometryConstants.GeometryEpsilon &&
                            lB >= -GeometryConstants.GeometryEpsilon &&
                            lC >= -GeometryConstants.GeometryEpsilon)
                            return (int)MathF.Round(a.Value * lA + b.Value * lB + c.Value * lC);
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

                        if (!TryBarycentric(pa, pb, pc, point, out float lA, out float lB, out float lC))
                            continue;

                        float penalty = MathF.Max(0f, -lA) + MathF.Max(0f, -lB) + MathF.Max(0f, -lC);

                        float area2 = MathF.Abs(Cross(pb - pa, pc - pa));
                        if (area2 <= GeometryConstants.GeometryEpsilon) continue;

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
                return (int)MathF.Round(LerpOnSegment(nearest[0], nearest[1], point));

            return (int)MathF.Round(bestA.Value * bestLA + bestB.Value * bestLB + bestC.Value * bestLC);
        }

        private static float InterpolateTriangle(
            InfluenceSample<int> a,
            InfluenceSample<int> b,
            InfluenceSample<int> c,
            PointXY p)
        {
            if (!TryBarycentric(a.SourcePoint, b.SourcePoint, c.SourcePoint, p, out float lA, out float lB, out float lC))
                return LerpOnSegment(a, b, p);

            return a.Value * lA + b.Value * lB + c.Value * lC;
        }

        private static bool TryBarycentric(PointXY a, PointXY b, PointXY c, PointXY p,
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

        private static float LerpOnSegment(InfluenceSample<int> a, InfluenceSample<int> b, PointXY p)
        {
            var pa = a.SourcePoint;
            var pb = b.SourcePoint;

            var ab = pb - pa;
            var ap = p - pa;

            float ab2 = ab.X * ab.X + ab.Y * ab.Y;
            if (ab2 <= GeometryConstants.GeometryEpsilonSquared) return a.Value;

            float t = (ap.X * ab.X + ap.Y * ab.Y) / ab2;

            return a.Value * (1f - t) + b.Value * t;
        }

        private static float EffectiveDistance(InfluenceSample<int> sample)
        {
            float weight = MathF.Max(sample.Weight, WeightEpsilon);
            return sample.Distance / weight;
        }

        private static InfluenceSample<int>[] GetNearestSamples(InfluenceSample<int>[] samples, int count)
        {
            var nearest = new InfluenceSample<int>[count];
            var effectiveDistances = new float[count];

            for (int i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                float effectiveDistance = EffectiveDistance(sample);

                if (i < count || effectiveDistance < effectiveDistances[count - 1])
                    InsertNearest(nearest, effectiveDistances, System.Math.Min(i, count), sample, effectiveDistance);
            }

            return nearest;
        }

        private static void InsertNearest(
            InfluenceSample<int>[] nearest,
            float[] effectiveDistances,
            int existingCount,
            InfluenceSample<int> sample,
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

        private static float Cross(VectorXY u, VectorXY v) => u.X * v.Y - u.Y * v.X;
    }
}
