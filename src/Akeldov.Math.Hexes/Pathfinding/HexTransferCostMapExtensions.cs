using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Pathfinding
{
    /// <summary>
    /// Provides pathfinding operations over <see cref="HexTransferCostMap"/>.
    /// </summary>
    public static class HexTransferCostMapExtensions
    {
        /// <summary>
        /// Finds a path with the lowest total transfer cost between two cells.
        /// </summary>
        /// <param name="costs">The transfer costs used for each adjacent step.</param>
        /// <param name="from">The first index in the path.</param>
        /// <param name="to">The last index in the path.</param>
        /// <returns>
        /// The minimum-cost path, or <see langword="null"/> when no finite-cost path exists.
        /// </returns>
        /// <remarks>
        /// The search uses Dijkstra's algorithm. Every finite entry and exit cost must be non-negative.
        /// <see cref="float.PositiveInfinity"/> marks an entry or exit as impassable. When
        /// <paramref name="from"/> and <paramref name="to"/> are equal, the result contains that index
        /// and has zero total cost.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="costs"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="from"/> or <paramref name="to"/> lies outside the map topology.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an entry or exit cost is negative, <see cref="float.NaN"/>, or negative infinity.
        /// </exception>
        public static HexPath? FindShortestPath(
            this HexTransferCostMap costs,
            VectorXYInt from,
            VectorXYInt to)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            HexMapTopology topology = costs.Topology;
            if (from.X < 0 || from.X >= topology.Resolution.X ||
                from.Y < 0 || from.Y >= topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(from), from, "Source hex index must lie within the map topology.");

            if (to.X < 0 || to.X >= topology.Resolution.X ||
                to.Y < 0 || to.Y >= topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(to), to, "Destination hex index must lie within the map topology.");

            for (int index = 0; index < topology.Count; index++)
            {
                float exitCost = costs.ExitCosts[index];
                if (float.IsNaN(exitCost) || exitCost < 0f)
                    throw new InvalidOperationException($"Exit cost at flat index {index} must be non-negative and not NaN.");

                float entryCost = costs.EntryCosts[index];
                if (float.IsNaN(entryCost) || entryCost < 0f)
                    throw new InvalidOperationException($"Entry cost at flat index {index} must be non-negative and not NaN.");
            }

            if (from == to)
                return new HexPath(new[] { from }, 0f);

            int width = topology.Resolution.X;
            int sourceIndex = from.Y * width + from.X;
            int destinationIndex = to.Y * width + to.X;
            var distances = new float[topology.Count];
            var previous = new int[topology.Count];

            for (int index = 0; index < topology.Count; index++)
            {
                distances[index] = float.PositiveInfinity;
                previous[index] = -1;
            }

            distances[sourceIndex] = 0f;
            var queue = new MinPriorityQueue();
            queue.Enqueue(sourceIndex, 0f);

            while (queue.Count > 0)
            {
                QueueEntry current = queue.Dequeue();
                if (current.Cost > distances[current.Index])
                    continue;

                if (current.Index == destinationIndex)
                    break;

                var currentHex = new VectorXYInt(current.Index % width, current.Index / width);
                for (int directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    VectorXYInt adjacent = currentHex.GetAdjacent((SixfoldAngle)directionIndex, topology.Layout);
                    if (adjacent.X < 0 || adjacent.X >= topology.Resolution.X ||
                        adjacent.Y < 0 || adjacent.Y >= topology.Resolution.Y)
                        continue;

                    float stepCost = costs.ExitCosts[current.Index] + costs.EntryCosts[adjacent];
                    if (float.IsPositiveInfinity(stepCost))
                        continue;

                    float candidateCost = current.Cost + stepCost;
                    int adjacentIndex = adjacent.Y * width + adjacent.X;
                    if (candidateCost >= distances[adjacentIndex])
                        continue;

                    distances[adjacentIndex] = candidateCost;
                    previous[adjacentIndex] = current.Index;
                    queue.Enqueue(adjacentIndex, candidateCost);
                }
            }

            if (float.IsPositiveInfinity(distances[destinationIndex]))
                return null;

            int pathLength = 1;
            for (int index = destinationIndex; index != sourceIndex; index = previous[index])
                pathLength++;

            var path = new VectorXYInt[pathLength];
            int pathIndex = pathLength - 1;
            for (int index = destinationIndex; index >= 0; index = previous[index])
            {
                path[pathIndex--] = new VectorXYInt(index % width, index / width);
                if (index == sourceIndex)
                    break;
            }

            return new HexPath(path, distances[destinationIndex]);
        }

        private readonly struct QueueEntry
        {
            internal QueueEntry(int index, float cost, long order)
            {
                Index = index;
                Cost = cost;
                Order = order;
            }

            internal int Index { get; }

            internal float Cost { get; }

            internal long Order { get; }
        }

        private sealed class MinPriorityQueue
        {
            private readonly List<QueueEntry> _entries = new List<QueueEntry>();
            private long _nextOrder;

            internal int Count => _entries.Count;

            internal void Enqueue(int index, float cost)
            {
                var entry = new QueueEntry(index, cost, _nextOrder++);
                int entryIndex = _entries.Count;
                _entries.Add(entry);

                while (entryIndex > 0)
                {
                    int parentIndex = (entryIndex - 1) / 2;
                    if (!HasHigherPriority(entry, _entries[parentIndex]))
                        break;

                    _entries[entryIndex] = _entries[parentIndex];
                    entryIndex = parentIndex;
                }

                _entries[entryIndex] = entry;
            }

            internal QueueEntry Dequeue()
            {
                QueueEntry result = _entries[0];
                int lastIndex = _entries.Count - 1;
                QueueEntry last = _entries[lastIndex];
                _entries.RemoveAt(lastIndex);

                if (_entries.Count == 0)
                    return result;

                int entryIndex = 0;
                while (true)
                {
                    int leftIndex = entryIndex * 2 + 1;
                    if (leftIndex >= _entries.Count)
                        break;

                    int rightIndex = leftIndex + 1;
                    int childIndex = rightIndex < _entries.Count &&
                                     HasHigherPriority(_entries[rightIndex], _entries[leftIndex])
                        ? rightIndex
                        : leftIndex;

                    if (!HasHigherPriority(_entries[childIndex], last))
                        break;

                    _entries[entryIndex] = _entries[childIndex];
                    entryIndex = childIndex;
                }

                _entries[entryIndex] = last;
                return result;
            }

            private static bool HasHigherPriority(QueueEntry left, QueueEntry right) =>
                left.Cost < right.Cost ||
                left.Cost == right.Cost && left.Order < right.Order;
        }
    }
}
