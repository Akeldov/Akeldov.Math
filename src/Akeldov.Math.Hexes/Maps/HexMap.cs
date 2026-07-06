using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a HexMap instance.
    /// </summary>
    /// <typeparam name="TValue">The type of value handled by this member.</typeparam>
    public class HexMap<TValue> : IHexMap<TValue>
    {
        private readonly TValue[] _values;

        /// <summary>
        /// Performs the HexMap operation.
        /// </summary>
        /// <param name="topology">The topology value.</param>
        public HexMap(HexMapTopology topology)
            : this(new IndexSeptupletMap(topology))
        {
        }

        /// <summary>
        /// Performs the HexMap operation.
        /// </summary>
        /// <param name="topology">The topology value.</param>
        public HexMap(IndexSeptupletMap topology)
        {
            Topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _values = new TValue[checked(topology.Width * topology.Height)];
        }

        internal HexMap(IndexSeptupletMap topology, TValue[] values)
        {
            Topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _values = values ?? throw new ArgumentNullException(nameof(values));

            if (values.Length != topology.Width * topology.Height)
                throw new ArgumentException("Values length must match topology dimensions.", nameof(values));
        }

        /// <summary>
        /// Gets the Topology value.
        /// </summary>
        public IndexSeptupletMap Topology { get; }

        /// <summary>
        /// Gets the Width value.
        /// </summary>
        public int Width => Topology.Width;

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height => Topology.Height;

        /// <summary>
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout => Topology.Layout;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public TValue this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Width ||
                    index.Y < 0 || index.Y >= Topology.Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index.X < 0 || index.X >= Topology.Width ||
                    index.Y < 0 || index.Y >= Topology.Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                _values[GetFlatIndex(index)] = value;
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public TValue this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _values[index] = value;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Width + index.X;
    }
}
