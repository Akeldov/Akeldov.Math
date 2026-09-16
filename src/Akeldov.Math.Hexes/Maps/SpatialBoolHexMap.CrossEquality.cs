using System;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialBoolHexMap
    {
        /// <summary>Creates a spatial Boolean map identifying equal cells in spatial and topology-only maps.</summary>
        /// <param name="left">The spatial source map whose geometry is retained.</param>
        /// <param name="right">The topology-only source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static SpatialBoolHexMap operator ==(SpatialBoolHexMap left, BoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying equal cells in topology-only and spatial maps.</summary>
        /// <param name="left">The topology-only source map.</param>
        /// <param name="right">The spatial source map whose geometry is retained.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static SpatialBoolHexMap operator ==(BoolHexMap left, SpatialBoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying different cells in spatial and topology-only maps.</summary>
        /// <param name="left">The spatial source map whose geometry is retained.</param>
        /// <param name="right">The topology-only source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static SpatialBoolHexMap operator !=(SpatialBoolHexMap left, BoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying different cells in topology-only and spatial maps.</summary>
        /// <param name="left">The topology-only source map.</param>
        /// <param name="right">The spatial source map whose geometry is retained.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static SpatialBoolHexMap operator !=(BoolHexMap left, SpatialBoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }
    }
}
#pragma warning restore CS0660, CS0661
