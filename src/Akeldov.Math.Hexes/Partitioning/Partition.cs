using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a group of hex indexes stored as a read-only snapshot.
    /// </summary>
    /// <remarks>
    /// The input sequence is copied, preserving its order and any duplicate indexes.
    /// Empty partitions are allowed. Index bounds and membership in a particular map are not validated.
    /// </remarks>
    public class Partition : IPartition
    {
        private readonly IReadOnlyList<VectorXYInt> _hexIndexes;

        /// <summary>
        /// Initializes a new partition with a read-only copy of the supplied hex indexes.
        /// </summary>
        /// <param name="hexIndexes">The hex indexes to copy into the partition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexIndexes"/> is null.
        /// </exception>
        public Partition(IReadOnlyList<VectorXYInt> hexIndexes)
        {
            if (hexIndexes == null)
                throw new ArgumentNullException(nameof(hexIndexes));

            var copy = new VectorXYInt[hexIndexes.Count];
            for (int i = 0; i < hexIndexes.Count; i++)
                copy[i] = hexIndexes[i];

            _hexIndexes = Array.AsReadOnly(copy);
        }

        /// <summary>
        /// Gets the read-only structural snapshot of hex indexes belonging to this partition.
        /// </summary>
        /// <remarks>
        /// Changes to the constructor's input collection do not affect this sequence.
        /// </remarks>
        public IReadOnlyList<VectorXYInt> HexIndexes => _hexIndexes;
    }
}
