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
        private readonly TValue[] _values;

        /// <summary>
        /// Performs the HexMap operation.
        /// </summary>
        /// <param name="topology">The topology value.</param>
        public HexMap(HexMapTopology topology)
        {
            _topology = topology;
            _values = new TValue[topology.Count];
        }

        internal HexMap(HexMapTopology topology, TValue[] values)
        {
            _topology = topology;
            _values = values ?? throw new ArgumentNullException(nameof(values));

            if (values.Length != topology.Count)
                throw new ArgumentException("Values length must match topology dimensions.", nameof(values));
        }

        /// <summary>
        /// Gets the Topology value.
        /// </summary>
        public HexMapTopology Topology => _topology;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
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

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;
    }
}
