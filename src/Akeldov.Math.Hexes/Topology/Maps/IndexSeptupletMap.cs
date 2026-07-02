using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexSeptupletMap : IHexMap<Septuplet<VectorXYInt>>
    {
        private readonly Septuplet<VectorXYInt>[] _adjacent;

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
            _adjacent = new Septuplet<VectorXYInt>[checked(width * height)];
            Adjacent = Array.AsReadOnly(_adjacent);

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

        public int Width { get; }

        public int Height { get; }

        public int Count => _adjacent.Length;

        public Layout Layout { get; }

        public IReadOnlyList<Septuplet<VectorXYInt>> Adjacent { get; }

        internal Septuplet<VectorXYInt>[] AdjacentStorage => _adjacent;

        public Septuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _adjacent[GetFlatIndex(index)];
            }
        }

        public Septuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adjacent[index];
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
                    _adjacent[rowStart + x] = CreateAdjacency(x, y, offsets);
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
                    _adjacent[rowStart + x] = CreateAdjacency(x, y, offsets);
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
