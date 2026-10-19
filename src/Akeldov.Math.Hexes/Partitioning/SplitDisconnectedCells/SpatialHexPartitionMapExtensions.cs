using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>Provides connectivity operations on full spatial hex partitions.</summary>
    public static class SpatialHexPartitionMapExtensions
    {
        /// <summary>Splits each partition cell into its six-connected components while preserving spatial geometry.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full spatial partition whose assignments must agree with its retained cells.</param>
        /// <returns>
        /// A new mutable spatial partition map owned by the caller, with the same geometry and one new
        /// <see cref="HexPartitionCell"/> per non-empty connected component. The source map and
        /// its cells are unchanged. Source-specific cell metadata is not copied.
        /// </returns>
        /// <remarks>
        /// Connectivity uses the six edge-adjacent hexes without wrapping at map boundaries.
        /// Adjacent hexes belonging to different source cells remain in different result cells.
        /// Result cells receive consecutive identifiers starting at zero, in row-major component
        /// discovery order, independently of source identifiers and cell order. Empty source cells
        /// are omitted. Each result cell stores its hex indexes in breadth-first traversal order.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition is null.</exception>
        /// <exception cref="ArgumentException">Source cells or assignments are inconsistent.</exception>
        public static SpatialHexPartitionMap<HexPartitionCell> SplitDisconnectedCells<THexPartitionCell>(
            this SpatialHexPartitionMap<THexPartitionCell> partition)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));

            // Revalidate custom cells and check the mutable assignments against their current snapshot.
            var snapshot = new HexPartitionMap<THexPartitionCell>(partition.Topology, partition.Cells);
            for (int i = 0; i < snapshot.Topology.Count; i++)
            {
                if (partition[i] != snapshot[i])
                    throw new ArgumentException("Partition assignments must agree with the retained cell indexes.", nameof(partition));
            }

            return new SpatialHexPartitionMap<HexPartitionCell>(partition.Geometry,
                HexPartitionMapExtensions.SplitConnectedComponents(snapshot));
        }
    }
}
