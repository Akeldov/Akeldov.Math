using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexSeptupletGrid : IGrid<Septuplet<VectorXYInt>>
    {
        private Septuplet<VectorXYInt>[] _adjacent;

        public IndexSeptupletGrid(
            IndexSeptupletMap hexAdjacencyMap,
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

            _adjacent = new Septuplet<VectorXYInt>[checked(Width * Height)];
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

        public Septuplet<VectorXYInt> this[VectorXYInt index]
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

        public Septuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adjacent[index];
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;

        private void Fill(
            IndexSeptupletMap hexAdjacencyMap,
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
                    var cellIndex = new PointXY(x, y).ToXYIndex(radius, origin, layout);
                    _adjacent[index] = CreateValue(hexAdjacencyMap, cellIndex);
                    index = index + 1;
                }
            }
        }

        private static Septuplet<VectorXYInt> CreateValue(
            IndexSeptupletMap hexAdjacencyMap,
            VectorXYInt cellIndex)
        {
            if (ContainsCell(hexAdjacencyMap, cellIndex))
                return hexAdjacencyMap[cellIndex];

            sbyte[] offsets = HexAdjacencyOffsets.GetOffsets(hexAdjacencyMap.Layout, cellIndex.X, cellIndex.Y);
            return new Septuplet<VectorXYInt>(
                cellIndex,
                new VectorXYInt(cellIndex.X + offsets[0], cellIndex.Y + offsets[1]),
                new VectorXYInt(cellIndex.X + offsets[2], cellIndex.Y + offsets[3]),
                new VectorXYInt(cellIndex.X + offsets[4], cellIndex.Y + offsets[5]),
                new VectorXYInt(cellIndex.X + offsets[6], cellIndex.Y + offsets[7]),
                new VectorXYInt(cellIndex.X + offsets[8], cellIndex.Y + offsets[9]),
                new VectorXYInt(cellIndex.X + offsets[10], cellIndex.Y + offsets[11]));
        }

        private static bool ContainsCell(IndexSeptupletMap hexAdjacencyMap, VectorXYInt cellIndex)
        {
            return (uint)cellIndex.X < (uint)hexAdjacencyMap.Width &&
                (uint)cellIndex.Y < (uint)hexAdjacencyMap.Height;
        }
    }
}
