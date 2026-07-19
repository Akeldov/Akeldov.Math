using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Stores one mutable value for every cell in a rectangular hex-map topology.
    /// </summary>
    /// <typeparam name="TValue">The type stored in each hex cell.</typeparam>
    public class HexMap<TValue> : IHexMap<TValue>
    {
        private readonly HexMapTopology _topology;
        private readonly TValue[] _values;

        /// <summary>
        /// Initializes an empty map whose cells contain the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="topology">The layout and resolution of the map.</param>
        public HexMap(HexMapTopology topology)
        {
            _topology = topology;
            _values = new TValue[topology.Count];
        }

        /// <summary>
        /// Initializes a new hex map that uses the specified array as its backing storage.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        /// <param name="values">
        /// The backing array. Its length must equal the number of cells in <paramref name="topology"/>.
        /// Values must be stored in row-major order: X advances first, and the value at coordinates
        /// <c>(x, y)</c> is stored at <c>y * topology.Resolution.X + x</c>.
        /// </param>
        /// <remarks>
        /// <b>Ownership warning:</b> the array is retained by the map and is not copied. The caller and
        /// the map share the same mutable storage, so changes made through either one are visible through
        /// the other. Do not reuse or modify the array independently when exclusive map ownership is required.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="values"/> does not match the topology cell count.
        /// </exception>
        public HexMap(HexMapTopology topology, TValue[] values)
        {
            _topology = topology;
            _values = values ?? throw new ArgumentNullException(nameof(values));

            if (values.Length != topology.Count)
                throw new ArgumentException("Values length must match topology dimensions.", nameof(values));
        }

        /// <summary>
        /// Gets the layout and resolution shared by all cells in the map.
        /// </summary>
        public HexMapTopology Topology => _topology;

        /// <summary>
        /// Gets the value at the specified hex coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public TValue this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                _values[GetFlatIndex(index)] = value;
            }
        }

        /// <summary>
        /// Gets the value at the specified flat index. Flat indexes use row-major order: X advances
        /// first, and coordinates <c>(x, y)</c> map to <c>y * Topology.Resolution.X + x</c>.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        public TValue this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _values[index] = value;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;
    }
}
