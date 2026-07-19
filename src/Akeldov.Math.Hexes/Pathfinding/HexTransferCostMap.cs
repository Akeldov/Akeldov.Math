using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Pathfinding
{
    /// <summary>
    /// Provides the cost of transferring directly between any two cells of a hex map.
    /// </summary>
    /// <remarks>
    /// A transfer cost is the sum of the source cell's exit cost and the destination cell's entry cost.
    /// No adjacency, distance, or pathfinding cost is applied.
    /// </remarks>
    public sealed class HexTransferCostMap
    {
        /// <summary>
        /// Initializes a transfer-cost map from matching entry- and exit-cost maps.
        /// </summary>
        /// <param name="exitCosts">The exit cost of each source cell.</param>
        /// <param name="entryCosts">The entry cost of each destination cell.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="exitCosts"/> or <paramref name="entryCosts"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the maps do not have the same topology.
        /// </exception>
        public HexTransferCostMap(IHexMap<float> exitCosts, IHexMap<float> entryCosts)
        {
            ExitCosts = exitCosts ?? throw new ArgumentNullException(nameof(exitCosts));
            EntryCosts = entryCosts ?? throw new ArgumentNullException(nameof(entryCosts));

            if (exitCosts.Topology != entryCosts.Topology)
                throw new ArgumentException("Entry and exit cost maps must have the same topology.", nameof(entryCosts));
        }

        /// <summary>
        /// Gets the exit-cost map retained by this instance.
        /// </summary>
        public IHexMap<float> ExitCosts { get; }

        /// <summary>
        /// Gets the entry-cost map retained by this instance.
        /// </summary>
        public IHexMap<float> EntryCosts { get; }

        /// <summary>
        /// Gets the topology shared by the entry- and exit-cost maps.
        /// </summary>
        public HexMapTopology Topology => ExitCosts.Topology;

        /// <summary>
        /// Gets the cost of transferring directly from one cell to another.
        /// </summary>
        /// <param name="from">The cell whose exit cost is used.</param>
        /// <param name="to">The cell whose entry cost is used.</param>
        /// <returns>The source exit cost plus the destination entry cost.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="from"/> or <paramref name="to"/> lies outside the map topology.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetTransferCost(VectorXYInt from, VectorXYInt to)
        {
            if (from.X < 0 || from.X >= Topology.Resolution.X ||
                from.Y < 0 || from.Y >= Topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(from), from, "Source hex index must lie within the map topology.");

            if (to.X < 0 || to.X >= Topology.Resolution.X ||
                to.Y < 0 || to.Y >= Topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(to), to, "Destination hex index must lie within the map topology.");

            return ExitCosts[from] + EntryCosts[to];
        }
    }
}
