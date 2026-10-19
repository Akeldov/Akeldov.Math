using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>Provides connectivity operations on partial spatial hex partitions.</summary>
    public static class SpatialPartialHexPartitionMapExtensions
    {
        /// <summary>Splits each assigned partition cell into its six-connected components, preserving geometry and unassigned hexes.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial spatial partition whose assignments must agree with its retained cells.</param>
        /// <returns>
        /// A new mutable partial spatial partition map owned by the caller, with the same geometry and one new
        /// <see cref="HexPartitionCell"/> per non-empty connected component. Unassigned hexes remain null.
        /// The source map and its cells are unchanged. Source-specific cell metadata is not copied.
        /// </returns>
        /// <remarks>
        /// Connectivity uses the six edge-adjacent hexes without wrapping at map boundaries.
        /// Unassigned hexes do not connect components and do not become result cells.
        /// Adjacent hexes belonging to different source cells remain in different result cells.
        /// Result cells receive consecutive identifiers starting at zero, in row-major component
        /// discovery order, independently of source identifiers and cell order. Empty source cells
        /// are omitted. Each result cell stores its hex indexes in breadth-first traversal order.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition is null.</exception>
        /// <exception cref="ArgumentException">Source cells or assignments are inconsistent.</exception>
        public static SpatialPartialHexPartitionMap<HexPartitionCell> SplitDisconnectedCells<THexPartitionCell>(
            this SpatialPartialHexPartitionMap<THexPartitionCell> partition)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));

            // Revalidate custom cells and check all assignments, including unassigned hexes.
            var snapshot = new PartialHexPartitionMap<THexPartitionCell>(partition.Topology, partition.Cells);
            for (int i = 0; i < snapshot.Topology.Count; i++)
            {
                if (partition[i] != snapshot[i])
                    throw new ArgumentException("Partition assignments must agree with the retained cell indexes.", nameof(partition));
            }

            return new SpatialPartialHexPartitionMap<HexPartitionCell>(partition.Geometry,
                HexPartitionMapExtensions.SplitConnectedComponents(snapshot));
        }
    }
}
