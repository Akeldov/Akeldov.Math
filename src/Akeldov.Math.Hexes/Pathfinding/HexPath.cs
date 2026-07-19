using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Pathfinding
{
    /// <summary>
    /// Represents a path through a hex map and its total transfer cost.
    /// </summary>
    public sealed class HexPath
    {
        internal HexPath(IReadOnlyList<VectorXYInt> hexIndexes, float totalCost)
        {
            var copy = new VectorXYInt[hexIndexes.Count];
            for (int i = 0; i < hexIndexes.Count; i++)
                copy[i] = hexIndexes[i];

            HexIndexes = Array.AsReadOnly(copy);
            TotalCost = totalCost;
        }

        /// <summary>
        /// Gets the read-only semantic result of hex indexes from the source through the destination.
        /// </summary>
        public IReadOnlyList<VectorXYInt> HexIndexes { get; }

        /// <summary>
        /// Gets the sum of transfer costs between consecutive indexes in the path.
        /// </summary>
        public float TotalCost { get; }
    }
}
