using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>Constructs bounded Euclidean Voronoi diagrams for point sites.</summary>
    public static class RectangleExtensions
    {
        /// <summary>Partitions a rectangle and applies the specified policy to cells with no area.</summary>
        /// <param name="bounds">The rectangle to partition.</param>
        /// <param name="sites">Distinct finite sites, in result order.</param>
        /// <param name="emptyCellPolicy">How to handle empty cells. The default keeps them.</param>
        /// <returns>A new, mutable list owned by the caller, with retained cells in input order.</returns>
        /// <exception cref="InvalidOperationException">A cell is empty and the policy is <see cref="EmptyCellPolicy.ThrowException"/>.</exception>
#pragma warning disable RS0026 // Both overloads intentionally allow omitting the final empty-cell policy.
#pragma warning disable MA0016 // The caller owns the mutable result list.
        public static List<VoronoiCell> PartitionByVoronoi(
            this Rectangle bounds,
            IReadOnlyList<PointXY> sites,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs)
            => bounds.PartitionByVoronoi(sites, 0, emptyCellPolicy);
#pragma warning restore MA0016, RS0026

        /// <summary>Partitions a rectangle, relaxes its sites, then applies the empty-cell policy.</summary>
        /// <param name="bounds">The rectangle to partition.</param>
        /// <param name="sites">Initial distinct finite sites, in result order.</param>
        /// <param name="relaxationIterations">Number of Lloyd relaxation steps. Zero uses the initial diagram.</param>
        /// <param name="emptyCellPolicy">How to handle cells with no area in the final diagram.
        /// During relaxation, an empty cell retains its site. The default keeps empty cells.</param>
        /// <returns>A new, mutable list owned by the caller, with retained cells in input order.</returns>
        /// <exception cref="ArgumentNullException">The site list is null.</exception>
        /// <exception cref="ArgumentException">Initial or relaxed site positions coincide.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative, a site coordinate
        /// is not finite, or the empty-cell policy is unrecognized.</exception>
        /// <exception cref="InvalidOperationException">A final cell is empty and the policy is <see cref="EmptyCellPolicy.ThrowException"/>.</exception>
#pragma warning disable RS0026 // Both overloads intentionally allow omitting the final empty-cell policy.
#pragma warning disable MA0016 // The caller owns the mutable result list.
        public static List<VoronoiCell> PartitionByVoronoi(
            this Rectangle bounds,
            IReadOnlyList<PointXY> sites,
            int relaxationIterations,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs)
#pragma warning restore MA0016, RS0026
        {
            var cells = BuildRelaxedCells(bounds, sites, relaxationIterations);
            switch (emptyCellPolicy)
            {
                case EmptyCellPolicy.LeaveAsIs:
                    break;
                case EmptyCellPolicy.Exclude:
                    cells.RemoveAll(cell => cell.Vertices.Count == 0);
                    break;
                case EmptyCellPolicy.ThrowException:
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (cells[i].Vertices.Count == 0)
                            throw new InvalidOperationException($"Cell {i} at site {cells[i].Site} has no area.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emptyCellPolicy));
            }

            return cells;
        }

        private static List<VoronoiCell> BuildCells(Rectangle bounds, IReadOnlyList<PointXY> sites)
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));

            var unique = new HashSet<PointXY>();
            foreach (var site in sites)
            {
                PointXYValidation.ThrowIfNotFinite(site, nameof(sites), "Site coordinates must be finite.");
                if (!unique.Add(site))
                    throw new ArgumentException("Site positions must be distinct.", nameof(sites));
            }

            var cells = new List<VoronoiCell>(sites.Count);
            var polygon = new List<(double X, double Y)>();
            var clipped = new List<(double X, double Y)>();
            foreach (var site in sites)
            {
                polygon.Clear();
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    polygon.Add((bounds.Min.X, bounds.Min.Y));
                    polygon.Add((bounds.Max.X, bounds.Min.Y));
                    polygon.Add((bounds.Max.X, bounds.Max.Y));
                    polygon.Add((bounds.Min.X, bounds.Max.Y));
                }

                foreach (var other in sites)
                {
                    if (polygon.Count == 0)
                        break;
                    if (site.Equals(other))
                        continue;

                    clipped.Clear();
                    ClipToBisector(polygon, clipped, site, other);
                    (polygon, clipped) = (clipped, polygon);
                }

                cells.Add(new VoronoiCell(site, ToVertices(polygon)));
            }

            return cells;
        }

        private static List<VoronoiCell> BuildRelaxedCells(Rectangle bounds, IReadOnlyList<PointXY> sites, int relaxationIterations)
        {
            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            var cells = BuildCells(bounds, sites);
            if (relaxationIterations == 0 || cells.Count == 0)
                return cells;

            var relaxedSites = new PointXY[cells.Count];
            for (int iteration = 0; iteration < relaxationIterations; iteration++)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = cells[i];
                    relaxedSites[i] = cell.Vertices.Count == 0 ? cell.Site : cell.GetCentroid();
                }

                cells = BuildCells(bounds, relaxedSites);
            }

            return cells;
        }

        private static void ClipToBisector(List<(double X, double Y)> polygon,
            List<(double X, double Y)> output, PointXY site, PointXY other)
        {
            double dx = (double)other.X - site.X;
            double dy = (double)other.Y - site.Y;
            double midX = ((double)other.X + site.X) * 0.5;
            double midY = ((double)other.Y + site.Y) * 0.5;
            double Side((double X, double Y) p) => (p.X - midX) * dx + (p.Y - midY) * dy;

            var previous = polygon[polygon.Count - 1];
            double previousSide = Side(previous);
            foreach (var current in polygon)
            {
                double currentSide = Side(current);
                if ((previousSide < 0 && currentSide > 0) || (previousSide > 0 && currentSide < 0))
                {
                    double denominator = previousSide - currentSide;
                    double a = -currentSide / denominator;
                    double b = previousSide / denominator;
                    output.Add((a * previous.X + b * current.X, a * previous.Y + b * current.Y));
                }

                if (currentSide <= 0)
                    output.Add(current);
                previous = current;
                previousSide = currentSide;
            }
        }

        private static List<PointXY> ToVertices(List<(double X, double Y)> polygon)
        {
            var vertices = new List<PointXY>(polygon.Count);
            foreach (var p in polygon)
            {
                var point = new PointXY((float)p.X, (float)p.Y);
                if (vertices.Count == 0 || !vertices[vertices.Count - 1].Equals(point))
                    vertices.Add(point);
            }

            if (vertices.Count > 1 && vertices[0].Equals(vertices[vertices.Count - 1]))
                vertices.RemoveAt(vertices.Count - 1);

            double twiceArea = 0;
            for (int i = 2; i < vertices.Count; i++)
            {
                var origin = vertices[0];
                var a = vertices[i - 1];
                var b = vertices[i];
                twiceArea += ((double)a.X - origin.X) * ((double)b.Y - origin.Y)
                    - ((double)a.Y - origin.Y) * ((double)b.X - origin.X);
            }

            if (twiceArea <= 0)
                vertices.Clear();
            return vertices;
        }
    }
}
