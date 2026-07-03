using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the IndexSeptupletMap type.
    /// </summary>
    public sealed class IndexSeptupletMap : IHexMap<Septuplet<VectorXYInt>>
    {
        private readonly Septuplet<VectorXYInt>[] _values;

        /// <summary>
        /// Initializes a new instance of the IndexSeptupletMap type.
        /// </summary>
        /// <param name="width">The Width value.</param>
        /// <param name="height">The Height value.</param>
        /// <param name="layout">The Layout value.</param>
        public IndexSeptupletMap(
            int width,
            int height,
            Layout layout)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            Layout = layout;
            _values = new Septuplet<VectorXYInt>[checked(width * height)];

            switch (layout)
            {
                case Layout.OddR:
                    FillRowLayoutTopology(false);
                    break;
                case Layout.EvenR:
                    FillRowLayoutTopology(true);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutTopology(false);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutTopology(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        /// <summary>
        /// Gets the Width value.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the Count value.
        /// </summary>
        public int Count => _values.Length;

        /// <summary>
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout { get; }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Septuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Septuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;

        private void FillRowLayoutTopology(bool evenRowsAreShifted)
        {
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;
                var offsets = HexAdjacencyOffsets.GetRowOffsets(y, evenRowsAreShifted);

                for (int x = 0; x < Width; x++)
                {
                    _values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private void FillColumnLayoutTopology(bool evenColumnsAreShifted)
        {
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;

                for (int x = 0; x < Width; x++)
                {
                    var offsets = HexAdjacencyOffsets.GetColumnOffsets(x, evenColumnsAreShifted);
                    _values[rowStart + x] = CreateAdjacency(x, y, offsets);
                }
            }
        }

        private static Septuplet<VectorXYInt> CreateAdjacency(
            int x,
            int y,
            sbyte[] offsets)
        {
            return new Septuplet<VectorXYInt>(
                new VectorXYInt(x, y),
                new VectorXYInt(x + offsets[0], y + offsets[1]),
                new VectorXYInt(x + offsets[2], y + offsets[3]),
                new VectorXYInt(x + offsets[4], y + offsets[5]),
                new VectorXYInt(x + offsets[6], y + offsets[7]),
                new VectorXYInt(x + offsets[8], y + offsets[9]),
                new VectorXYInt(x + offsets[10], y + offsets[11]));
        }
    }
}
