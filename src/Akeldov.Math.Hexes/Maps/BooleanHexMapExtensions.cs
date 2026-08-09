using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides Boolean operations for hex maps.
    /// </summary>
    public static class BooleanHexMapExtensions
    {
        /// <summary>
        /// Creates a hex map whose cells contain the conjunction of the corresponding cells in
        /// two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static HexMap<bool> And(
            this IHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            return new HexMap<bool>(left.Topology, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of the corresponding
        /// cells in two spatial source maps.
        /// </summary>
        /// <param name="left">The first spatial source map.</param>
        /// <param name="right">The second spatial source map.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this ISpatialHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of corresponding cells
        /// in a spatial source map and a topology-compatible source map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The source map to combine with <paramref name="left"/>.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="right"/> is spatial and its geometry differs from <paramref name="left"/>.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this ISpatialHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (right is ISpatialHexMap<bool> spatialRight && left.Geometry != spatialRight.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of corresponding cells
        /// in a topology-compatible source map and a spatial source map.
        /// </summary>
        /// <param name="left">The source map to combine with <paramref name="right"/>.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="left"/> is spatial and its geometry differs from <paramref name="right"/>.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this IHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (left is ISpatialHexMap<bool> spatialLeft && spatialLeft.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(right.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a hex map whose cells contain the disjunction of the corresponding cells in
        /// two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static HexMap<bool> Or(
            this IHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            return new HexMap<bool>(left.Topology, CreateDisjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the disjunction of the corresponding
        /// cells in two spatial source maps.
        /// </summary>
        /// <param name="left">The first spatial source map.</param>
        /// <param name="right">The second spatial source map.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialHexMap<bool> Or(
            this ISpatialHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateDisjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the disjunction of corresponding cells
        /// in a spatial source map and a topology-compatible source map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The source map to combine with <paramref name="left"/>.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="right"/> is spatial and its geometry differs from <paramref name="left"/>.
        /// </exception>
        public static SpatialHexMap<bool> Or(
            this ISpatialHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (right is ISpatialHexMap<bool> spatialRight && left.Geometry != spatialRight.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateDisjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the disjunction of corresponding cells
        /// in a topology-compatible source map and a spatial source map.
        /// </summary>
        /// <param name="left">The source map to combine with <paramref name="right"/>.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="left"/> is spatial and its geometry differs from <paramref name="right"/>.
        /// </exception>
        public static SpatialHexMap<bool> Or(
            this IHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (left is ISpatialHexMap<bool> spatialLeft && spatialLeft.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(right.Geometry, CreateDisjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a floating-point hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The Boolean condition map. A <see langword="true"/> cell selects the corresponding value
        /// from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable floating-point hex map owned by the caller. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap Select(
            this BoolHexMap condition,
            FloatHexMap whenTrue,
            FloatHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Topology != whenTrue.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenTrue));

            if (condition.Topology != whenFalse.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenFalse));

            var values = new float[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new FloatHexMap(condition.Topology, values);
        }

        /// <summary>
        /// Creates an integer hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The Boolean condition map. A <see langword="true"/> cell selects the corresponding value
        /// from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable integer hex map owned by the caller. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static IntHexMap Select(
            this BoolHexMap condition,
            IntHexMap whenTrue,
            IntHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Topology != whenTrue.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenTrue));

            if (condition.Topology != whenFalse.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenFalse));

            var values = new int[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new IntHexMap(condition.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <param name="condition">
        /// The Boolean condition map. A <see langword="true"/> cell selects the corresponding value
        /// from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. None of the source maps is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap Select(
            this BoolHexMap condition,
            BoolHexMap whenTrue,
            BoolHexMap whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Topology != whenTrue.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenTrue));

            if (condition.Topology != whenFalse.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenFalse));

            var values = new bool[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new BoolHexMap(condition.Topology, values);
        }

        /// <summary>
        /// Creates a hex map by selecting between two source maps cell by cell.
        /// </summary>
        /// <typeparam name="TValue">The type stored in each source and result cell.</typeparam>
        /// <param name="condition">
        /// The Boolean condition map. A <see langword="true"/> cell selects the corresponding value
        /// from <paramref name="whenTrue"/>; a <see langword="false"/> cell selects it from
        /// <paramref name="whenFalse"/>.
        /// </param>
        /// <param name="whenTrue">
        /// The source map selected where <paramref name="condition"/> is <see langword="true"/>.
        /// </param>
        /// <param name="whenFalse">
        /// The source map selected where <paramref name="condition"/> is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its backing storage is independent, but
        /// reference-type cell values are selected by reference and are not cloned. None of the
        /// source maps is modified.
        /// </returns>
        /// <remarks>
        /// The result is a non-spatial <see cref="HexMap{TValue}"/>. If a source map is a derived
        /// spatial map, its geometry is not copied.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="condition"/>, <paramref name="whenTrue"/>, or
        /// <paramref name="whenFalse"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static HexMap<TValue> Select<TValue>(
            this BoolHexMap condition,
            HexMap<TValue> whenTrue,
            HexMap<TValue> whenFalse)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));

            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            if (condition.Topology != whenTrue.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenTrue));

            if (condition.Topology != whenFalse.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(whenFalse));

            var values = new TValue[condition.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = condition[index] ? whenTrue[index] : whenFalse[index];

            return new HexMap<TValue>(condition.Topology, values);
        }

        private static bool[] CreateConjunctionValues(IHexMap<bool> left, IHexMap<bool> right)
        {
            var values = new bool[left.Topology.Count];
            for (int i = 0; i < values.Length; i++)
                values[i] = left[i] & right[i];

            return values;
        }

        private static bool[] CreateDisjunctionValues(IHexMap<bool> left, IHexMap<bool> right)
        {
            var values = new bool[left.Topology.Count];
            for (int i = 0; i < values.Length; i++)
                values[i] = left[i] | right[i];

            return values;
        }
    }
}
