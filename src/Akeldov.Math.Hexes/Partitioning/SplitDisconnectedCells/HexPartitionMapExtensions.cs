using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>Provides connectivity operations on full hex partitions.</summary>
    public static class HexPartitionMapExtensions
    {
        /// <summary>Splits each partition cell into its six-connected components.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <returns>
        /// A new mutable partition map owned by the caller, with the same topology and one new
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
        public static HexPartitionMap<HexPartitionCell> SplitDisconnectedCells<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition)
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

            return new HexPartitionMap<HexPartitionCell>(snapshot.Topology, SplitConnectedComponents(snapshot));
        }

        internal static List<HexPartitionCell> SplitConnectedComponents<TIdentifier>(IHexMap<TIdentifier> snapshot)
        {
            var topology = snapshot.Topology;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            var evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            var oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            var visited = new bool[topology.Count];
            var cells = new List<HexPartitionCell>();
            var component = new List<VectorXYInt>();

            for (int seed = 0; seed < topology.Count; seed++)
            {
                var sourceId = snapshot[seed];
                if (visited[seed] || sourceId is null)
                    continue;
                component.Clear();
                component.Add(new VectorXYInt(seed % width, seed / width));
                visited[seed] = true;

                for (int head = 0; head < component.Count; head++)
                {
                    var current = component[head];
                    int axis = parityUsesY ? current.Y : current.X;
                    var offsets = (axis & 1) == 0 ? evenOffsets : oddOffsets;
                    for (int direction = 0; direction < offsets.Length; direction++)
                    {
                        int x = current.X + offsets[direction].X;
                        int y = current.Y + offsets[direction].Y;
                        if ((uint)x >= (uint)width || (uint)y >= (uint)height)
                            continue;

                        int neighbor = y * width + x;
                        if (visited[neighbor] || !EqualityComparer<TIdentifier>.Default.Equals(snapshot[neighbor], sourceId))
                            continue;

                        visited[neighbor] = true;
                        component.Add(new VectorXYInt(x, y));
                    }
                }

                cells.Add(new HexPartitionCell(cells.Count, component));
            }

            return cells;
        }
    }
}
