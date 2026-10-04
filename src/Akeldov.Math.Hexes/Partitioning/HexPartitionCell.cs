using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents one cell of a hex-grid partition, storing its hex indexes as a read-only snapshot.
    /// </summary>
    /// <remarks>
    /// The input sequence is copied, preserving its order and any duplicate indexes.
    /// Empty cells are allowed. Index bounds and membership in a particular map are not validated.
    /// </remarks>
    public class HexPartitionCell : IHexPartitionCell
    {
        private readonly IReadOnlyList<VectorXYInt> _hexIndexes;

        /// <summary>
        /// Initializes a new partition cell with a read-only copy of the supplied hex indexes.
        /// </summary>
        /// <param name="id">The non-negative cell identifier, unique within its partition.</param>
        /// <param name="hexIndexes">The hex indexes to copy into the partition cell.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="id"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexIndexes"/> is null.
        /// </exception>
        public HexPartitionCell(int id, IReadOnlyList<VectorXYInt> hexIndexes)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            if (hexIndexes == null)
                throw new ArgumentNullException(nameof(hexIndexes));

            var copy = new VectorXYInt[hexIndexes.Count];
            for (int i = 0; i < hexIndexes.Count; i++)
                copy[i] = hexIndexes[i];

            _hexIndexes = Array.AsReadOnly(copy);
            Id = id;
        }

        /// <inheritdoc/>
        public int Id { get; }

        /// <summary>
        /// Gets the read-only structural snapshot of hex indexes belonging to this partition cell.
        /// </summary>
        /// <remarks>
        /// Changes to the constructor's input collection do not affect this sequence.
        /// </remarks>
        public IReadOnlyList<VectorXYInt> HexIndexes => _hexIndexes;
    }
}
