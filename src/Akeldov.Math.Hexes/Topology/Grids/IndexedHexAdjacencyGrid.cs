using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexedHexAdjacencyGrid : IHexMap<IndexedHexAdjacency>
    {
        private IndexedHexAdjacency[] _adjacent;

        public IndexedHexAdjacencyGrid(
            IndexedHexAdjacencyMap indexedHexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (indexedHexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyMap));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            Resolution = resolution;
            Width = resolution.X;
            Height = resolution.Y;

            var radius = 1f;
            var apothem = radius.ConvertHexRadiusToApothem();
            var boundingBoxSize = indexedHexAdjacencyMap.GetBoundingBoxSize(radius);
            var stepX = boundingBoxSize.X / Width;
            var stepY = boundingBoxSize.Y / Height;

            _adjacent = new IndexedHexAdjacency[checked(Width * Height)];
            switch (indexedHexAdjacencyMap.Layout)
            {
                case Layout.OddR:
                    Fill(Layout.OddR, new VectorXY(apothem, radius));
                    break;
                case Layout.EvenR:
                    Fill(Layout.EvenR, new VectorXY(2f * apothem, radius));
                    break;
                case Layout.OddQ:
                    Fill(Layout.OddQ, new VectorXY(radius, apothem));
                    break;
                case Layout.EvenQ:
                    Fill(Layout.EvenQ, new VectorXY(radius, 2f * apothem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indexedHexAdjacencyMap.Layout));
            }

            void Fill(Layout layout, VectorXY origin)
            {
                var index = 0;

                for (int i = 0; i < Height; i++)
                {
                    var y = (i + 0.5f) * stepY;

                    for (int j = 0; j < Width; j++)
                    {
                        var x = (j + 0.5f) * stepX;
                        var cellIndex = new VectorXY(x, y).ToXYIndex(radius, origin, layout);
                        _adjacent[index] = CreateValue(cellIndex, index);
                        index = index + 1;
                    }
                }
            }

            IndexedHexAdjacency CreateValue(VectorXYInt cellIndex, int index)
            {
                var flags = IndexedHexAdjacencyFlags.None;
                var flatIndex = index;
                var adjacent0Index = index;
                var adjacent1Index = index;
                var adjacent2Index = index;
                var adjacent3Index = index;
                var adjacent4Index = index;
                var adjacent5Index = index;

                if (ContainsCell(cellIndex))
                {
                    var adjacency = indexedHexAdjacencyMap[cellIndex];
                    flags = adjacency.Flags;
                    flatIndex = adjacency.Index;
                    adjacent0Index = adjacency.Adjacent0Index;
                    adjacent1Index = adjacency.Adjacent1Index;
                    adjacent2Index = adjacency.Adjacent2Index;
                    adjacent3Index = adjacency.Adjacent3Index;
                    adjacent4Index = adjacency.Adjacent4Index;
                    adjacent5Index = adjacency.Adjacent5Index;
                }
                else
                {
                    var offsets = HexAdjacencyOffsets.GetOffsets(indexedHexAdjacencyMap.Layout, cellIndex.X, cellIndex.Y);
                    TryAssignAdjacent(cellIndex, offsets, 0, IndexedHexAdjacencyFlags.Adjacent0, ref flags, ref adjacent0Index);
                    TryAssignAdjacent(cellIndex, offsets, 1, IndexedHexAdjacencyFlags.Adjacent1, ref flags, ref adjacent1Index);
                    TryAssignAdjacent(cellIndex, offsets, 2, IndexedHexAdjacencyFlags.Adjacent2, ref flags, ref adjacent2Index);
                    TryAssignAdjacent(cellIndex, offsets, 3, IndexedHexAdjacencyFlags.Adjacent3, ref flags, ref adjacent3Index);
                    TryAssignAdjacent(cellIndex, offsets, 4, IndexedHexAdjacencyFlags.Adjacent4, ref flags, ref adjacent4Index);
                    TryAssignAdjacent(cellIndex, offsets, 5, IndexedHexAdjacencyFlags.Adjacent5, ref flags, ref adjacent5Index);
                }

                return new IndexedHexAdjacency(
                    flags,
                    flatIndex,
                    adjacent0Index,
                    adjacent1Index,
                    adjacent2Index,
                    adjacent3Index,
                    adjacent4Index,
                    adjacent5Index);
            }

            void TryAssignAdjacent(
                VectorXYInt cellIndex,
                sbyte[] offsets,
                int adjacentOffset,
                IndexedHexAdjacencyFlags flag,
                ref IndexedHexAdjacencyFlags flags,
                ref int adjacentIndex)
            {
                var offsetIndex = adjacentOffset * 2;
                var adjacentCellIndex = new VectorXYInt(
                    cellIndex.X + offsets[offsetIndex],
                    cellIndex.Y + offsets[offsetIndex + 1]);

                if (!ContainsCell(adjacentCellIndex))
                    return;

                flags |= flag;
                adjacentIndex = indexedHexAdjacencyMap[adjacentCellIndex].Index;
            }

            bool ContainsCell(VectorXYInt cellIndex)
            {
                return (uint)cellIndex.X < (uint)indexedHexAdjacencyMap.Width &&
                    (uint)cellIndex.Y < (uint)indexedHexAdjacencyMap.Height;
            }
        }

        public VectorXYInt Resolution { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Count => _adjacent.Length;

        public IndexedHexAdjacency this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");

                return _adjacent[GetFlatIndex(index)];
            }
        }

        public IndexedHexAdjacency this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adjacent[index];
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;
    }
}
