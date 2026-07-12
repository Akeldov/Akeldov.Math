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
        private readonly HexMapTopology _topology;
        private readonly int _width;
        private readonly int _height;
        private readonly Layout _layout;

        private readonly TValue[] _values;

        /// <summary>
        /// Performs the HexMap operation.
        /// </summary>
        /// <param name="topology">The topology value.</param>
        public HexMap(HexMapTopology topology)
        {
            _topology = topology;
            _width = topology.Width;
            _height = topology.Height;
            _layout = topology.Layout;
            _values = new TValue[checked(topology.Width * topology.Height)];
        }

        /// <summary>
        /// Performs the HexMap operation.
        /// </summary>
        /// <param name="width">Width of the HexMap in hexes.</param>
        /// <param name="height">Height of the HexMap in hexes.</param>
        /// <param name="layout">Layout of the HexMap in hexes.</param>
        public HexMap(int width, int height, Layout layout)
        {
            _topology = new HexMapTopology(width, height, layout);
            _width = width;
            _height = height;
            _layout = layout;
            _values = new TValue[checked(width * height)];
        }

        internal HexMap(HexMapTopology topology, TValue[] values)
        {
            _topology = topology;
            _width = topology.Width;
            _height = topology.Height;
            _layout = topology.Layout;
            _values = values ?? throw new ArgumentNullException(nameof(values));

            if (values.Length != topology.Width * topology.Height)
                throw new ArgumentException("Values length must match topology dimensions.", nameof(values));
        }

        internal HexMap(int width, int height, Layout layout, TValue[] values)
        {
            _topology = new HexMapTopology(width, height, layout);
            _width = width;
            _height = height;
            _layout = layout;
            _values = values ?? throw new ArgumentNullException(nameof(values));

            if (values.Length != width * height)
                throw new ArgumentException("Values length must match topology dimensions.", nameof(values));
        }

        /// <summary>
        /// Gets the Topology value.
        /// </summary>
        public HexMapTopology Topology => _topology;

        /// <summary>
        /// Gets the Width value.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height => _height;

        /// <summary>
        /// Gets the map resolution in hexes.
        /// </summary>
        public VectorXYInt Resolution => new VectorXYInt(_width, _height);

        /// <summary>
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout => _layout;

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
