using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>Constructs bounded Euclidean Voronoi diagrams for point sites.</summary>
    public static class VoronoiDiagram
    {
        /// <summary>Partitions a rectangle by Euclidean distance to the given sites.</summary>
        /// <param name="bounds">The rectangle to partition.</param>
        /// <param name="sites">Distinct finite sites, which may lie outside the rectangle.</param>
        /// <returns>A new, mutable list owned by the caller, with one cell per site in input order.</returns>
        /// <exception cref="ArgumentNullException">The site list is null.</exception>
        /// <exception cref="ArgumentException">Sites have duplicate positions.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A site coordinate is not finite.</exception>
#pragma warning disable MA0016 // The caller owns the mutable result list.
        public static List<VoronoiCell> PartitionByVoronoi(Rectangle bounds, IReadOnlyList<PointXY> sites)
#pragma warning restore MA0016
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
