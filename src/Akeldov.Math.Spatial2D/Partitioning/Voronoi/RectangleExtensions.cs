using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>Constructs bounded Voronoi diagrams for unweighted and multiplicatively weighted point sites.</summary>
    public static class RectangleExtensions
    {
        /// <summary>Partitions a rectangle and applies the specified policy to cells with no area.</summary>
        /// <param name="bounds">The rectangle to partition.</param>
        /// <param name="sites">Distinct finite sites, in result order.</param>
        /// <param name="emptyCellPolicy">How to handle empty cells. The default keeps them.</param>
        /// <returns>A new, mutable list owned by the caller, with retained cells in input order.</returns>
        /// <exception cref="ArgumentException">A cell perimeter exceeds the finite range of <see cref="float"/>.</exception>
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
        /// <exception cref="ArgumentException">Initial or relaxed site positions coincide, or a cell perimeter
        /// exceeds the finite range of <see cref="float"/>.</exception>
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
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));
            var weighted = new Site[sites.Count];
            for (int i = 0; i < weighted.Length; i++)
                weighted[i] = new Site(sites[i], 1f);
            return bounds.PartitionByVoronoi(weighted, relaxationIterations, emptyCellPolicy);
        }

        /// <summary>Partitions a rectangle by distance divided by site weight.</summary>
        /// <param name="bounds">The rectangle to partition.</param>
        /// <param name="sites">Distinct weighted sites, in result order.</param>
        /// <param name="emptyCellPolicy">How to handle cells with no area.</param>
        /// <returns>A new, mutable list of cells owned by the caller.</returns>
#pragma warning disable RS0026, MA0016
        public static List<VoronoiCell> PartitionByVoronoi(this Rectangle bounds, IReadOnlyList<Site> sites,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs)
            => bounds.PartitionByVoronoi(sites, 0, emptyCellPolicy);

        /// <summary>Builds multiplicatively weighted Voronoi regions, optionally applying Lloyd relaxation.</summary>
        /// <param name="bounds">The clipping rectangle.</param>
        /// <param name="sites">Distinct weighted sites. Zero weights have no area. If any weights are infinite,
        /// only those sites have area and compete by ordinary distance.</param>
        /// <param name="relaxationIterations">Number of centroid updates. Weights persist and empty sites stay in place.</param>
        /// <param name="emptyCellPolicy">How to handle cells with no area after relaxation.</param>
        /// <returns>A new, mutable list owned by the caller, with retained cells in input order.</returns>
        /// <exception cref="ArgumentException">A nonempty input has only zero weights, site positions coincide,
        /// or a contour length exceeds the finite range of <see cref="float"/>.</exception>
        public static List<VoronoiCell> PartitionByVoronoi(this Rectangle bounds, IReadOnlyList<Site> sites,
            int relaxationIterations, EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs)
#pragma warning restore RS0026, MA0016
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));
            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            var cells = BuildCells(bounds, sites);
            if (relaxationIterations > 0 && cells.Count > 0)
            {
                var relaxed = new Site[cells.Count];
                for (int iteration = 0; iteration < relaxationIterations; iteration++)
                {
                    for (int i = 0; i < cells.Count; i++)
                    {
                        var cell = cells[i];
                        relaxed[i] = new Site(cell.Region == null ? cell.Site : cell.GetCentroid(), cell.Weight);
                    }
                    cells = BuildCells(bounds, relaxed);
                }
            }

            switch (emptyCellPolicy)
            {
                case EmptyCellPolicy.LeaveAsIs:
                    break;
                case EmptyCellPolicy.Exclude:
                    cells.RemoveAll(cell => cell.Region == null);
                    break;
                case EmptyCellPolicy.ThrowException:
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (cells[i].Region == null)
                            throw new InvalidOperationException($"Cell {i} at site {cells[i].Site} has no area.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emptyCellPolicy));
            }

            return cells;
        }

        private static List<VoronoiCell> BuildCells(Rectangle bounds, IReadOnlyList<Site> sites)
        {
            var positions = new HashSet<PointXY>();
            float maxWeight = 0;
            foreach (var site in sites)
            {
                if (!positions.Add(site.Position))
                    throw new ArgumentException("Site positions must be distinct.", nameof(sites));
                maxWeight = MathF.Max(maxWeight, site.Weight);
            }
            if (sites.Count > 0 && maxWeight == 0)
                throw new ArgumentException("At least one site weight must be positive.", nameof(sites));

            var cells = new List<VoronoiCell>(sites.Count);
            var polygon = new List<(double X, double Y)>();
            var clipped = new List<(double X, double Y)>();
            foreach (var site in sites)
                cells.Add(BuildCell(bounds, site, sites, maxWeight, polygon, clipped));
            return cells;
        }

        private static VoronoiCell BuildCell(Rectangle bounds, Site site, IReadOnlyList<Site> sites, float maxWeight,
            List<(double X, double Y)> polygon, List<(double X, double Y)> clipped)
        {
            if (site.Weight == 0 || (float.IsPositiveInfinity(maxWeight) && site.Weight != maxWeight))
                return new VoronoiCell(site, null, default);

            var circles = new List<BisectorCircle>();
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
                if (site.Position.Equals(other.Position) || other.Weight == 0)
                    continue;

                if (site.Weight != other.Weight)
                {
                    if (float.IsPositiveInfinity(site.Weight))
                        continue;
                    var circle = new BisectorCircle(site, other);
                    var range = circle.Range(bounds);
                    if (circle.KeepInside ? range.Min >= 0 : range.Max <= 0)
                        return new VoronoiCell(site, null, default);
                    if (circle.KeepInside ? range.Max > 0 : range.Min < 0)
                        circles.Add(circle);
                    continue;
                }

                clipped.Clear();
                ClipToBisector(polygon, clipped, site.Position, other.Position);
                (polygon, clipped) = (clipped, polygon);
            }

            return WeightedCellGeometry.Build(site, bounds, ToVertices(polygon), circles);
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

            return vertices;
        }
    }
}
