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
    }
}
