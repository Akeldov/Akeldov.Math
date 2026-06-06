using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    public class HexMap<TValue> : IHexMap<TValue>
    {
        private readonly TValue[] _values;

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

        public IndexSeptupletMap Topology { get; }

        public int Width => Topology.Width;

        public int Height => Topology.Height;

        public Layout Layout => Topology.Layout;

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
