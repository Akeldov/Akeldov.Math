using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the BarycentricTripletGrid type.
    /// </summary>
    public sealed class BarycentricTripletGrid : ISpatialRaster<Triplet<float>>
    {
        private Triplet<float>[] _values = Array.Empty<Triplet<float>>();

        /// <summary>
        /// Initializes a new instance of the BarycentricTripletGrid type.
        /// </summary>
        /// <param name="hexMapGeometry">The hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public BarycentricTripletGrid(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;
            _values = new Triplet<float>[checked(Resolution.X * Resolution.Y)];

            Fill();
        }

        /// <summary>
        /// Gets the source hex map geometry sampled by the raster.
        /// </summary>
        public HexMapGeometry SourceHexMapGeometry { get; }

        /// <summary>
        /// Gets the geometry used to sample the raster.
        /// </summary>
        public RasterGeometry Geometry { get; }

        /// <summary>
        /// Gets the Resolution value.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

        /// <summary>
        /// Gets the value at the specified grid coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public Triplet<float> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Triplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfGridIndexOutOfBounds(index);
                return _values[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Triplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get a value at the specified index.
        /// </summary>
        /// <param name="gridIndex">The gridIndex value.</param>
        /// <param name="barycentricCoordinates">The barycentricCoordinates value.</param>
        public bool TryGetValue(VectorXYInt gridIndex, out Triplet<float> barycentricCoordinates)
        {
            if (!ContainsGridIndex(gridIndex))
            {
                barycentricCoordinates = default;
                return false;
            }

            int flatIndex = GetFlatIndex(gridIndex);
            barycentricCoordinates = _values[flatIndex];
            return true;
        }

        private void Fill()
        {
            switch (SourceHexMapGeometry.Topology.Layout)
            {
                case Layout.OddR:
                    FillOddR();
                    break;
                case Layout.EvenR:
                    FillEvenR();
                    break;
                case Layout.OddQ:
                    FillOddQ();
                    break;
                case Layout.EvenQ:
                    FillEvenQ();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SourceHexMapGeometry.Topology.Layout));
            }
        }

        private void FillOddR()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.OddR);
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(Layout.OddR);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(Layout.OddR);
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    _values[index] = CreateOddR(new PointXY(pointX, pointY), vertices, evenOffsets, oddOffsets);
                }
            }
        }

        private void FillEvenR()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.EvenR);
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(Layout.EvenR);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(Layout.EvenR);
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    _values[index] = CreateEvenR(new PointXY(pointX, pointY), vertices, evenOffsets, oddOffsets);
                }
            }
        }

        private void FillOddQ()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.OddQ);
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(Layout.OddQ);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(Layout.OddQ);
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    _values[index] = CreateOddQ(new PointXY(pointX, pointY), vertices, evenOffsets, oddOffsets);
                }
            }
        }

        private void FillEvenQ()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout.EvenQ);
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(Layout.EvenQ);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(Layout.EvenQ);
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    _values[index] = CreateEvenQ(new PointXY(pointX, pointY), vertices, evenOffsets, oddOffsets);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Triplet<float> CreateOddR(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToOddRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetOddRHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[(vertex + 1) % 6];
            VectorXYInt rightIndex = mainIndex + offsets[vertex];
            return point.BarycentricCoordinates(mainCenter, GetOddRHexCenter(leftIndex), GetOddRHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Triplet<float> CreateEvenR(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToEvenRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetEvenRHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[(vertex + 1) % 6];
            VectorXYInt rightIndex = mainIndex + offsets[vertex];
            return point.BarycentricCoordinates(mainCenter, GetEvenRHexCenter(leftIndex), GetEvenRHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Triplet<float> CreateOddQ(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToOddQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetOddQHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[vertex];
            VectorXYInt rightIndex = mainIndex + offsets[(vertex + 5) % 6];
            return point.BarycentricCoordinates(mainCenter, GetOddQHexCenter(leftIndex), GetOddQHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Triplet<float> CreateEvenQ(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToEvenQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetEvenQHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[vertex];
            VectorXYInt rightIndex = mainIndex + offsets[(vertex + 5) % 6];
            return point.BarycentricCoordinates(mainCenter, GetEvenQHexCenter(leftIndex), GetEvenQHexCenter(rightIndex));
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (!ContainsGridIndex(index))
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }

        private bool ContainsGridIndex(VectorXYInt index)
        {
            return (uint)index.X < (uint)Resolution.X &&
                (uint)index.Y < (uint)Resolution.Y;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Resolution.X + index.X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VectorXY GetOddRHexCenter(VectorXYInt index) => new VectorXY(
            SourceHexMapGeometry.Origin.X + index.X * 2f * SourceHexMapGeometry.Apothem + ((index.Y & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f),
            SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * index.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VectorXY GetEvenRHexCenter(VectorXYInt index) => new VectorXY(
            SourceHexMapGeometry.Origin.X + index.X * 2f * SourceHexMapGeometry.Apothem + ((index.Y & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f),
            SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * index.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VectorXY GetOddQHexCenter(VectorXYInt index) => new VectorXY(
            SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * index.X,
            SourceHexMapGeometry.Origin.Y + index.Y * 2f * SourceHexMapGeometry.Apothem + ((index.X & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VectorXY GetEvenQHexCenter(VectorXYInt index) => new VectorXY(
            SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * index.X,
            SourceHexMapGeometry.Origin.Y + index.Y * 2f * SourceHexMapGeometry.Apothem + ((index.X & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f));

        private static int GetClosestVertexIndex(
            PointXY point,
            VectorXY hexCenter,
            float hexRadius,
            VectorXY[] normalizedHexVertices,
            out float minSquaredDistance)
        {
            minSquaredDistance = float.MaxValue;
            int closestVertexIndex = 0;

            for (int i = 0; i < normalizedHexVertices.Length; i++)
            {
                VectorXY vertex = hexCenter + normalizedHexVertices[i] * hexRadius;
                float squaredDistance = SquaredDistance(point, vertex);

                if (squaredDistance < minSquaredDistance)
                {
                    minSquaredDistance = squaredDistance;
                    closestVertexIndex = i;
                }
            }

            return closestVertexIndex;
        }

        private static float SquaredDistance(PointXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }

    }
}
