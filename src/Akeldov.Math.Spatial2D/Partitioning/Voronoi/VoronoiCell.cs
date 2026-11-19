using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>A Euclidean Voronoi cell clipped to the partition bounds.</summary>
    public sealed class VoronoiCell
    {
        internal VoronoiCell(PointXY site, List<PointXY> vertices)
        {
            Site = site;
            Vertices = vertices.AsReadOnly();
        }

        /// <summary>The point generating this cell.</summary>
        public PointXY Site { get; }

        /// <summary>The cell boundary in counterclockwise order, without a repeated closing vertex.
        /// This structural view is empty when the clipped cell has no representable area.</summary>
        public IReadOnlyList<PointXY> Vertices { get; }

        /// <summary>Computes the area centroid of the clipped cell.</summary>
        /// <returns>The center of mass of the cell assuming uniform density.</returns>
        /// <exception cref="InvalidOperationException">The cell has no area.</exception>
        public PointXY GetCentroid()
        {
            if (Vertices.Count == 0)
                throw new InvalidOperationException("A cell with no area has no centroid.");

            // Use the first vertex as a local origin for the area-weighted triangle fan.
            var origin = Vertices[0];
            double totalDoubleArea = 0;
            double weightedX = 0;
            double weightedY = 0;
            for (int i = 2; i < Vertices.Count; i++)
            {
                var a = Vertices[i - 1];
                var b = Vertices[i];
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
