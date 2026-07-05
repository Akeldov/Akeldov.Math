using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Selects point influence sources by excluding sources hidden behind half-plane boundaries.
    /// </summary>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    public class HalfPlaneCuller<TPointSource> : IInfluenceSourceCuller<TPointSource>
        where TPointSource : IPointInfluenceSource
    {
        private readonly IReadOnlyList<TPointSource> _pointSources;

        /// <summary>
        /// Initializes a new half-plane influence source culler.
        /// </summary>
        /// <param name="pointSources">The point influence sources available for culling.</param>
        public HalfPlaneCuller(IReadOnlyList<TPointSource> pointSources)
        {
            if (pointSources == null)
                throw new ArgumentNullException(nameof(pointSources));

            if (pointSources.Count == 0)
                throw new ArgumentException("Influence source collection must not be empty.", nameof(pointSources));

            var copy = new TPointSource[pointSources.Count];
            for (int i = 0; i < pointSources.Count; i++)
            {
                var pointSource = pointSources[i];
                if (pointSource is null)
                    throw new ArgumentException("Influence source collection cannot contain null elements.", nameof(pointSources));

                if (!PointXYValidation.IsFinite(pointSource.Position))
                    throw new ArgumentException("Influence source positions must be finite.", nameof(pointSources));

                copy[i] = pointSource;
            }

            _pointSources = copy;
        }

        /// <summary>
        /// Returns influence sources that are visible from the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>A new mutable list of visible influence sources owned by the caller.</returns>
        public List<TPointSource> Cull(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var orderedPointSources = OrderBy(_pointSources, x => x.Position.Distance(point));
            var culledPointSources = new List<TPointSource>();
            var lines = new List<Line>();
            for (int i = 0; i < orderedPointSources.Count; i++)
            {
                var pointSource = orderedPointSources[i];
                bool isExcluded = false;

                for (int j = 0; j < lines.Count; j++)
                {
                    var line = lines[j];
                    if (!line.IsSameSide(point, pointSource.Position))
                    {
                        isExcluded = true;
                        break;
                    }
                }

                if (!isExcluded)
                {
                    culledPointSources.Add(pointSource);

                    if (pointSource.Position.Distance(point) <= GeometryConstants.GeometryEpsilon)
                        continue;

                    var line = new Line(point, pointSource.Position);
                    var perpendicular = line.PerpendicularAt(pointSource.Position);
                    lines.Add(perpendicular);
                }
            }

            return culledPointSources;
        }

        private List<TEntity> OrderBy<TEntity, TProperty>(
            IReadOnlyList<TEntity> source, Func<TEntity, TProperty> selector)
            where TProperty : IComparable<TProperty>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new List<TEntity>(source);

            result.Sort((x, y) =>
            {
                return (selector(x)).CompareTo(selector(y));
            });

            return result;
        }

    }
}
