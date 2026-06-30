using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores the Voronoi cell assigned to each hex center.
    /// </summary>
    public sealed class VoronoiHexPartitionMap : HexMap<VoronoiCell>
    {
        internal VoronoiHexPartitionMap(HexCenterMap centers, VoronoiCell[] assignments, VoronoiCell[] cells)
            : base(CreateTopology(centers), assignments)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            Centers = centers;
            Cells = Array.AsReadOnly(cells);
        }

        /// <summary>
        /// Gets the hex center map used to create this partition map.
        /// </summary>
        public HexCenterMap Centers { get; }

        /// <summary>
        /// Gets the read-only semantic result of Voronoi cells, one per source site.
        /// </summary>
        public IReadOnlyList<VoronoiCell> Cells { get; }

        private static IndexSeptupletMap CreateTopology(HexCenterMap centers)
        {
            if (centers == null)
                throw new ArgumentNullException(nameof(centers));

            return new IndexSeptupletMap(centers.Width, centers.Height, centers.Layout);
        }
    }
}
