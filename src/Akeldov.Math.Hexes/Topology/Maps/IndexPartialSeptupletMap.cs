using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexPartialSeptupletMap : IHexMap<PartialSeptuplet<VectorXYInt>>
    {
        private readonly PartialSeptuplet<VectorXYInt>[] _indexPartialSeptuplet;

        public IndexPartialSeptupletMap(
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
            _indexPartialSeptuplet = new PartialSeptuplet<VectorXYInt>[checked(width * height)];

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

        public int Count => _indexPartialSeptuplet.Length;

        public Layout Layout { get; }

        internal PartialSeptuplet<VectorXYInt>[] IndexPartialSeptuplet => _indexPartialSeptuplet;

        public PartialSeptuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _indexPartialSeptuplet[GetFlatIndex(index)];
            }
        }

        public PartialSeptuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indexPartialSeptuplet[index];
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
                    _indexPartialSeptuplet[rowStart + x] = CreateAdjacency(x, y, Width, Height, offsets);
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
                    _indexPartialSeptuplet[rowStart + x] = CreateAdjacency(x, y, Width, Height, offsets);
                }
            }
        }

        private static PartialSeptuplet<VectorXYInt> CreateAdjacency(
            int x,
            int y,
            int width,
            int height,
            sbyte[] offsets)
        {
            var adjacency = new Septuplet<VectorXYInt>(
                new VectorXYInt(x, y),
                new VectorXYInt(x + offsets[0], y + offsets[1]),
                new VectorXYInt(x + offsets[2], y + offsets[3]),
                new VectorXYInt(x + offsets[4], y + offsets[5]),
                new VectorXYInt(x + offsets[6], y + offsets[7]),
                new VectorXYInt(x + offsets[8], y + offsets[9]),
                new VectorXYInt(x + offsets[10], y + offsets[11]));

            SeptupletPresenceFlags presence = SeptupletPresenceFlags.Main;

            if (ContainsIndex(adjacency.Adjacent0, width, height))
                presence |= SeptupletPresenceFlags.Adjacent0;

            if (ContainsIndex(adjacency.Adjacent1, width, height))
                presence |= SeptupletPresenceFlags.Adjacent1;

            if (ContainsIndex(adjacency.Adjacent2, width, height))
                presence |= SeptupletPresenceFlags.Adjacent2;

            if (ContainsIndex(adjacency.Adjacent3, width, height))
                presence |= SeptupletPresenceFlags.Adjacent3;

            if (ContainsIndex(adjacency.Adjacent4, width, height))
                presence |= SeptupletPresenceFlags.Adjacent4;

            if (ContainsIndex(adjacency.Adjacent5, width, height))
                presence |= SeptupletPresenceFlags.Adjacent5;

            return new PartialSeptuplet<VectorXYInt>(adjacency, presence);
        }

        private static bool ContainsIndex(VectorXYInt index, int width, int height)
        {
            return (uint)index.X < (uint)width &&
                   (uint)index.Y < (uint)height;
        }
    }
}
