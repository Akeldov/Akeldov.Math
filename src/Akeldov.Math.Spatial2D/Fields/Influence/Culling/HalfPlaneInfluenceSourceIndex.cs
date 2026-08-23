using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Selects point influence sources by excluding sources hidden behind half-plane boundaries.
    /// </summary>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    public sealed class HalfPlaneInfluenceSourceIndex<TPointSource> : IInfluenceSourceIndex<TPointSource>
        where TPointSource : IPointInfluenceSource
    {
        private readonly TPointSource[] _pointSources;
        private readonly IReadOnlyList<TPointSource> _readOnlyPointSources;

        /// <summary>
        /// Initializes a new half-plane influence source index.
        /// </summary>
        /// <param name="pointSources">The point influence sources used to create the immutable snapshot.</param>
        public HalfPlaneInfluenceSourceIndex(IReadOnlyList<TPointSource> pointSources)
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
            _readOnlyPointSources = Array.AsReadOnly(copy);
        }

        /// <summary>
        /// Gets the immutable source snapshot owned by this index.
        /// </summary>
        public IReadOnlyList<TPointSource> Sources => _readOnlyPointSources;

        /// <summary>
        /// Returns influence sources that are visible from the specified point.
        /// </summary>
        /// <param name="point">The point being sampled.</param>
        /// <returns>A new mutable list of visible sources owned by the caller.</returns>
        public List<TPointSource> SelectSources(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            List<int> selectedIndexes = SelectSourceIndexes(_pointSources, point);
            var selectedSources = new List<TPointSource>(selectedIndexes.Count);
            for (int i = 0; i < selectedIndexes.Count; i++)
                selectedSources.Add(_pointSources[selectedIndexes[i]]);

            return selectedSources;
        }

        internal static List<int> SelectSourceIndexes(IReadOnlyList<TPointSource> pointSources, PointXY point)
        {
            var orderedIndexes = new List<int>(pointSources.Count);
            for (int i = 0; i < pointSources.Count; i++)
                orderedIndexes.Add(i);

            orderedIndexes.Sort((left, right) =>
            {
                int comparison = pointSources[left].Position.Distance(point)
                    .CompareTo(pointSources[right].Position.Distance(point));
                return comparison != 0 ? comparison : left.CompareTo(right);
            });

            var selectedIndexes = new List<int>();
            var lines = new List<Line>();
            for (int i = 0; i < orderedIndexes.Count; i++)
            {
                int sourceIndex = orderedIndexes[i];
                TPointSource pointSource = pointSources[sourceIndex];
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
                    selectedIndexes.Add(sourceIndex);

                    if (pointSource.Position.Distance(point) <= GeometryConstants.GeometryEpsilon)
                        continue;

                    var line = new Line(point, pointSource.Position);
                    var perpendicular = line.PerpendicularAt(pointSource.Position);
                    lines.Add(perpendicular);
                }
            }

            return selectedIndexes;
        }
    }
}
