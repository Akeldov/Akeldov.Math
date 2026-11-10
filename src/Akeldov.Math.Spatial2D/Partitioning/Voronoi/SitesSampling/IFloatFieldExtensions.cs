using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;
using System;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Provides Poisson disk sampling of weighted Voronoi sites from floating-point fields.
    /// </summary>
    public static class IFloatFieldExtensions
    {
        /// <summary>
        /// Generates weighted Voronoi sites within the specified bounds using a spatially varying minimal distance field.
        /// </summary>
        /// <param name="minimalDistanceField">
        /// The minimal distance field sampled in world coordinates.
        /// Its minimum and maximum must be finite, positive, and ordered, and sampled values must
        /// be finite, positive, and within that inclusive range. Distances use the same units as the coordinates.
        /// Larger values require greater spacing between sites.
        /// </param>
        /// <param name="bounds">The sampling rectangle in world coordinates, with finite positive width and height.</param>
        /// <param name="maxPoissonSamplingAttempts">The positive maximum number of candidates to try around each active point.</param>
        /// <param name="random">The random number generator used for sampling.</param>
        /// <returns>
        /// A new array of weighted Voronoi sites owned by the caller. Site positions are in world coordinates,
        /// and each site's <see cref="Site.Weight"/> is the minimal distance sampled at its position.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="minimalDistanceField"/> or <paramref name="random"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxPoissonSamplingAttempts"/> is not positive,
        /// a dimension of <paramref name="bounds"/> is not finite and positive,
        /// or the field's minimum and maximum are not finite, positive, and ordered.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the field returns a non-finite or non-positive value, or sampling produces no sites.
        /// </exception>
        public static Site[] SamplePoissonDiskSites(this IFloatField minimalDistanceField, Rectangle bounds, int maxPoissonSamplingAttempts, Random random)
        {
            if (minimalDistanceField == null)
                throw new ArgumentNullException(nameof(minimalDistanceField));

            var sampler = new PoissonDiskPointSampler(random, maxPoissonSamplingAttempts);
            // The sampler's rectangle starts at zero; the minimal distance field and sites use world coordinates.
            var samples = sampler.Sample(bounds.Size, new TranslatedFloatField(minimalDistanceField, bounds.Min));
            if (samples.Count == 0)
                throw new InvalidOperationException("Cell sampling did not produce any sites.");

            var sitesCount = samples.Count;
            var sites = new Site[sitesCount];
            for (int i = 0; i < sitesCount; i++)
            {
                var point = samples[i].Point;
                sites[i] = new Site(new PointXY(point.X + bounds.Min.X, point.Y + bounds.Min.Y), samples[i].MinimalDistance);
            }

            return sites;
        }

        private sealed class TranslatedFloatField : IFloatField
        {
            private readonly IFloatField _field;
            private readonly PointXY _origin;

            public TranslatedFloatField(IFloatField field, PointXY origin)
            {
                _field = field;
                _origin = origin;
            }

            public float Min => _field.Min;

            public float Max => _field.Max;

            public float Sample(PointXY point) =>
                _field.Sample(new PointXY(point.X + _origin.X, point.Y + _origin.Y));
        }
    }
}
