using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexedHexAdjacencyGrid : IGrid<Septuplet<int>>
    {
        private Septuplet<int>[] _adjacent;

        public IndexedHexAdjacencyGrid(
            HexAdjacencyMap hexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            Resolution = resolution;
            Width = resolution.X;
            Height = resolution.Y;

            var radius = 1f;
            var apothem = radius.ConvertHexRadiusToApothem();
            var boundingBoxSize = hexAdjacencyMap.GetBoundingBoxSize(radius);
            var stepX = boundingBoxSize.X / Width;
            var stepY = boundingBoxSize.Y / Height;

            _adjacent = new Septuplet<int>[checked(Width * Height)];
            switch (hexAdjacencyMap.Layout)
            {
                case Layout.OddR:
                    Fill(hexAdjacencyMap, radius, stepX, stepY, Layout.OddR, new VectorXY(apothem, radius));
                    break;
                case Layout.EvenR:
                    Fill(hexAdjacencyMap, radius, stepX, stepY, Layout.EvenR, new VectorXY(2f * apothem, radius));
                    break;
                case Layout.OddQ:
                    Fill(hexAdjacencyMap, radius, stepX, stepY, Layout.OddQ, new VectorXY(radius, apothem));
                    break;
                case Layout.EvenQ:
                    Fill(hexAdjacencyMap, radius, stepX, stepY, Layout.EvenQ, new VectorXY(radius, 2f * apothem));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hexAdjacencyMap.Layout));
            }
        }

        public VectorXYInt Resolution { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Count => _adjacent.Length;

        public Septuplet<int> this[VectorXYInt index]
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

        public Septuplet<int> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adjacent[index];
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;

        private void Fill(
            HexAdjacencyMap hexAdjacencyMap,
            float radius,
            float stepX,
            float stepY,
            Layout layout,
            VectorXY origin)
        {
            var index = 0;

            for (int i = 0; i < Height; i++)
            {
                var y = (i + 0.5f) * stepY;

                for (int j = 0; j < Width; j++)
                {
                    var x = (j + 0.5f) * stepX;
                    var cellIndex = new VectorXY(x, y).ToXYIndex(radius, origin, layout);
                    _adjacent[index] = CreateValue(hexAdjacencyMap, cellIndex, index);
                    index = index + 1;
                }
            }
        }

        private Septuplet<int> CreateValue(
            HexAdjacencyMap hexAdjacencyMap,
            VectorXYInt cellIndex,
            int index)
        {
            var flatIndex = -1;
            var adjacent0Index = -1;
            var adjacent1Index = -1;
            var adjacent2Index = -1;
            var adjacent3Index = -1;
            var adjacent4Index = -1;
            var adjacent5Index = -1;

            if (ContainsCell(hexAdjacencyMap, cellIndex))
            {
                var adjacency = hexAdjacencyMap[cellIndex];
                flatIndex = adjacency.Main;
                adjacent0Index = adjacency.Adjacent0;
                adjacent1Index = adjacency.Adjacent1;
                adjacent2Index = adjacency.Adjacent2;
                adjacent3Index = adjacency.Adjacent3;
                adjacent4Index = adjacency.Adjacent4;
                adjacent5Index = adjacency.Adjacent5;
            }
            else
            {
                var offsets = HexAdjacencyOffsets.GetOffsets(hexAdjacencyMap.Layout, cellIndex.X, cellIndex.Y);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 0, ref adjacent0Index);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 1, ref adjacent1Index);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 2, ref adjacent2Index);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 3, ref adjacent3Index);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 4, ref adjacent4Index);
                TryAssignAdjacent(hexAdjacencyMap, cellIndex, offsets, 5, ref adjacent5Index);
            }

            return new Septuplet<int>(
                flatIndex,
                adjacent0Index,
                adjacent1Index,
                adjacent2Index,
                adjacent3Index,
                adjacent4Index,
                adjacent5Index);
        }

        private void TryAssignAdjacent(
            HexAdjacencyMap hexAdjacencyMap,
            VectorXYInt cellIndex,
            sbyte[] offsets,
            int adjacentOffset,
            ref int adjacentIndex)
        {
            var offsetIndex = adjacentOffset * 2;
            var adjacentCellIndex = new VectorXYInt(
                cellIndex.X + offsets[offsetIndex],
                cellIndex.Y + offsets[offsetIndex + 1]);

            if (!ContainsCell(hexAdjacencyMap, adjacentCellIndex))
                return;

            adjacentIndex = hexAdjacencyMap[adjacentCellIndex].Main;
        }

        private bool ContainsCell(HexAdjacencyMap hexAdjacencyMap, VectorXYInt cellIndex)
        {
            return (uint)cellIndex.X < (uint)hexAdjacencyMap.Width &&
                (uint)cellIndex.Y < (uint)hexAdjacencyMap.Height;
        }
    }
}
