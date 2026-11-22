using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Sampling.Point.UniformRandom
{
    /// <summary>Draws independent, uniformly distributed points in a rectangle. Points may coincide.</summary>
    public sealed class UniformRandomPointSampler
    {
        private readonly Random _random;

        /// <summary>Creates a sampler using the supplied random number generator.</summary>
        /// <param name="random">The generator used for each draw. Supply a seeded generator for repeatable samples.</param>
        public UniformRandomPointSampler(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

#pragma warning disable MA0016 // Samples are mutable and owned by the caller.
        /// <summary>Samples a rectangle with its minimum corner at the origin.</summary>
        /// <param name="fieldSize">The finite, positive width and height.</param>
        /// <param name="count">The number of points to draw. Zero produces an empty list.</param>
        /// <returns>A new mutable list of the requested number of points owned by the caller.</returns>
        public List<PointXY> Sample(VectorXY fieldSize, int count)
        {
            if (!fieldSize.IsFinite || fieldSize.X <= 0f || fieldSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(fieldSize), "Field size must be finite and positive.");

            return Sample(new Rectangle(default, new PointXY(fieldSize.X, fieldSize.Y)), count);
        }

        /// <summary>Samples points in world coordinates within the given rectangle, including its boundary.</summary>
        /// <param name="bounds">The sampling rectangle, with positive width and height.</param>
        /// <param name="count">The number of points to draw. Zero produces an empty list.</param>
        /// <returns>A new mutable list of the requested number of points owned by the caller.</returns>
        public List<PointXY> Sample(Rectangle bounds, int count)
        {
            if (bounds.Width <= 0f || bounds.Height <= 0f)
                throw new ArgumentOutOfRangeException(nameof(bounds), "Sampling bounds must have positive width and height.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var samples = new List<PointXY>(count);
            var size = bounds.Size;
            for (int i = 0; i < count; i++)
            {
                float x = (float)_random.NextDouble() * size.X;
                float y = (float)_random.NextDouble() * size.Y;
                // The rounded field size can slightly exceed the distance between the corners.
                samples.Add(new PointXY(MathF.Min(bounds.Min.X + x, bounds.Max.X),
                    MathF.Min(bounds.Min.Y + y, bounds.Max.Y)));
            }
            return samples;
        }
#pragma warning restore MA0016
    }
}
