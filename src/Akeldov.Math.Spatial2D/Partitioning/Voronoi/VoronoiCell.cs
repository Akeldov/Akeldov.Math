using System;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>A Euclidean Voronoi cell clipped to the partition bounds.</summary>
    public sealed class VoronoiCell
    {
        internal VoronoiCell(PointXY site, CompositeContour? contour)
        {
            Site = site;
            Contour = contour;
        }

        /// <summary>The point generating this cell.</summary>
        public PointXY Site { get; }

        /// <summary>The closed boundary formed by the cell's straight edges in counterclockwise order,
        /// or <see langword="null"/> when the clipped cell has no representable area.</summary>
        public CompositeContour? Contour { get; }

        /// <summary>Computes the area centroid of the clipped cell.</summary>
        /// <returns>The center of mass of the cell assuming uniform density.</returns>
        /// <exception cref="InvalidOperationException">The cell has no area.</exception>
        public PointXY GetCentroid()
        {
            if (Contour == null)
                throw new InvalidOperationException("A cell with no area has no centroid.");

            // Use the first vertex as a local origin for the area-weighted triangle fan.
            var edges = Contour.Curves;
            var origin = edges[0].StartPoint;
            double totalDoubleArea = 0;
            double weightedX = 0;
            double weightedY = 0;
            for (int i = 1; i < edges.Count - 1; i++)
            {
                var a = edges[i].StartPoint;
                var b = edges[i].EndPoint;
                double ax = (double)a.X - origin.X;
                double ay = (double)a.Y - origin.Y;
                double bx = (double)b.X - origin.X;
                double by = (double)b.Y - origin.Y;
                double doubleArea = ax * by - ay * bx;
                totalDoubleArea += doubleArea;
                weightedX += (ax + bx) * doubleArea;
                weightedY += (ay + by) * doubleArea;
            }

            return new PointXY((float)(origin.X + weightedX / (3 * totalDoubleArea)),
                (float)(origin.Y + weightedY / (3 * totalDoubleArea)));
        }
    }
}
