using System;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Creates a spatial Boolean hex map whose cells contain the conjunction of the corresponding
        /// cells in two spatial Boolean source maps.
        /// </summary>
        /// <param name="left">The first spatial source map.</param>
        /// <param name="right">The second spatial source map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap And(
            this SpatialBoolHexMap left,
            SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialBoolHexMap(left.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial Boolean hex map whose cells contain the disjunction of the corresponding
        /// cells in two spatial Boolean source maps.
        /// </summary>
        /// <param name="left">The first spatial source map.</param>
        /// <param name="right">The second spatial source map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap Or(
            this SpatialBoolHexMap left,
            SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialBoolHexMap(left.Geometry, CreateDisjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial floating-point hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The spatial Boolean condition map. A <see langword="true"/> cell selects the corresponding
        /// value from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is taken from
        /// <paramref name="condition"/>. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialFloatHexMap Select(
            this SpatialBoolHexMap condition,
            SpatialFloatHexMap whenTrue,
            SpatialFloatHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Geometry != whenTrue.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenTrue));

            if (condition.Geometry != whenFalse.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenFalse));

            var values = new float[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new SpatialFloatHexMap(condition.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The spatial Boolean condition map. A <see langword="true"/> cell selects the corresponding
        /// value from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its geometry is taken from
        /// <paramref name="condition"/>. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialIntHexMap Select(
            this SpatialBoolHexMap condition,
            SpatialIntHexMap whenTrue,
            SpatialIntHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Geometry != whenTrue.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenTrue));

            if (condition.Geometry != whenFalse.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenFalse));

            var values = new int[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new SpatialIntHexMap(condition.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The spatial Boolean condition map. A <see langword="true"/> cell selects the corresponding
        /// value from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="condition"/>. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap Select(
            this SpatialBoolHexMap condition,
            SpatialBoolHexMap whenTrue,
            SpatialBoolHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Geometry != whenTrue.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenTrue));

            if (condition.Geometry != whenFalse.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(whenFalse));

            var values = new bool[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new SpatialBoolHexMap(condition.Geometry, values);
        }
    }
}
