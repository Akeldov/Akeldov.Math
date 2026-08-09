using Akeldov.Math.Spatial2D.Fields;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    /// <summary>
    /// Generates two-dimensional Poisson disk point samples.
    /// </summary>
    public class PoissonDiskPointSampler
    {
        private readonly Random _random;
        private readonly int _maxAttempts;

        /// <summary>
        /// Initializes a new Poisson disk point sampler.
        /// </summary>
        /// <param name="random">The random number generator used for sampling.</param>
        /// <param name="maxAttempts">The maximum number of candidates to try around each active point.</param>
        public PoissonDiskPointSampler(Random random, int maxAttempts)
        {
            if (maxAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));

            _random = random ?? throw new ArgumentNullException(nameof(random));
            _maxAttempts = maxAttempts;
        }

        /// <summary>
        /// Samples points in the rectangular field using a constant minimal distance.
        /// </summary>
        /// <param name="fieldSize">The finite positive field size.</param>
        /// <param name="minimalDistance">The finite positive minimal distance between samples.</param>
        /// <returns>A new mutable list of generated point samples owned by the caller.</returns>
        public List<PoissonDiskPointSample> Sample(VectorXY fieldSize, float minimalDistance)
        {
            if (minimalDistance <= 0f || float.IsNaN(minimalDistance) || float.IsInfinity(minimalDistance))
                throw new ArgumentOutOfRangeException(nameof(minimalDistance), "Minimal distance must be finite and positive.");

            return Sample(fieldSize, new ConstantFloatField(minimalDistance));
        }

        /// <summary>
        /// Samples points in the rectangular field using a spatially varying minimal distance field.
        /// </summary>
        /// <param name="fieldSize">The finite positive field size.</param>
        /// <param name="minimalDistanceField">The finite positive minimal distance field.</param>
        /// <returns>A new mutable list of generated point samples owned by the caller.</returns>
        public List<PoissonDiskPointSample> Sample(VectorXY fieldSize, IFloatField minimalDistanceField)
        {
            if (fieldSize.X <= 0f || float.IsNaN(fieldSize.X) || float.IsInfinity(fieldSize.X) ||
                fieldSize.Y <= 0f || float.IsNaN(fieldSize.Y) || float.IsInfinity(fieldSize.Y))
                throw new ArgumentOutOfRangeException(nameof(fieldSize), "Field size must be finite and positive.");

            if (minimalDistanceField == null)
                throw new ArgumentNullException(nameof(minimalDistanceField));

            float minimalDistanceMin = minimalDistanceField.Min;
            float minimalDistanceMax = minimalDistanceField.Max;
            if (minimalDistanceMin <= 0f || float.IsNaN(minimalDistanceMin) || float.IsInfinity(minimalDistanceMin) ||
                minimalDistanceMax <= 0f || float.IsNaN(minimalDistanceMax) || float.IsInfinity(minimalDistanceMax) ||
                minimalDistanceMin > minimalDistanceMax)
                throw new ArgumentOutOfRangeException(nameof(minimalDistanceField), "Minimal distance field range must be finite, positive, and ordered.");

            // The grid uses the maximum possible distance so every conflicting point must be in
            // the candidate cell or one of its immediate neighbours. Smaller cells require a
            // wider search radius to preserve the variable-distance invariant.
            float cellSize = minimalDistanceMax;

            var processList = new List<PointXY>();
            var samples = new List<PoissonDiskPointSample>();
            var grid = new Grid(fieldSize, cellSize);

            var first = _random.SelectRandomPoint(fieldSize);
            processList.Add(first);
            var firstMinimalDistance = SampleMinimalDistance(minimalDistanceField, first);

            if (grid.TryAdd(first, firstMinimalDistance))
            {
                samples.Add(new PoissonDiskPointSample(first, firstMinimalDistance));
            }

            while (processList.Count > 0)
            {
                int index = _random.Next(processList.Count);
                var currentPoint = processList[index];
                bool found = false;

                var minimalDistance = SampleMinimalDistance(minimalDistanceField, currentPoint);

                for (int i = 0; i < _maxAttempts; i++)
                {
                    var candidate = _random.SelectRandomAroundPoint(currentPoint, minimalDistance);
                    if (!grid.Contains(candidate))
                        continue;

                    var candidateMinimalDistance = SampleMinimalDistance(minimalDistanceField, candidate);

                    if (grid.TryAdd(candidate, candidateMinimalDistance))
                    {
                        samples.Add(new PoissonDiskPointSample(candidate, candidateMinimalDistance));
                        processList.Add(candidate);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    processList.RemoveAt(index);
            }

            return samples;
        }

        private static float SampleMinimalDistance(IFloatField minimalDistanceField, PointXY point)
        {
            float minimalDistance = minimalDistanceField.Sample(point);
            if (minimalDistance <= 0f || float.IsNaN(minimalDistance) || float.IsInfinity(minimalDistance))
                throw new InvalidOperationException("Minimal distance field returned an invalid value. Minimal distance must be finite and positive.");

            return minimalDistance;
        }
    }
}
