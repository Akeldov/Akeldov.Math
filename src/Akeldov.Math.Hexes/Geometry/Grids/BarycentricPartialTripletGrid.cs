using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents barycentric coordinates sampled from a bounded hex map onto a spatial raster.
    /// </summary>
    public sealed class BarycentricPartialTripletGrid : ISpatialRaster<PartialTriplet<float>>
    {
        private PartialTriplet<float>[] _values = Array.Empty<PartialTriplet<float>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BarycentricPartialTripletGrid"/> type.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public BarycentricPartialTripletGrid(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;
            _values = new PartialTriplet<float>[checked(Resolution.X * Resolution.Y)];

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
        /// Gets the raster resolution.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

        /// <summary>
        /// Gets the value at the specified raster coordinates.
        /// </summary>
        /// <param name="x">The horizontal raster coordinate.</param>
        /// <param name="y">The vertical raster coordinate.</param>
        public PartialTriplet<float> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified raster index.
        /// </summary>
        /// <param name="index">The raster index.</param>
        public PartialTriplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfGridIndexOutOfBounds(index);
                return _values[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the value at the specified flat raster index.
        /// </summary>
        /// <param name="index">The flat raster index.</param>
        public PartialTriplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get a present barycentric value at the specified raster index.
        /// </summary>
        /// <param name="gridIndex">The raster index.</param>
        /// <param name="barycentricCoordinates">The sampled barycentric coordinates.</param>
        public bool TryGetValue(VectorXYInt gridIndex, out PartialTriplet<float> barycentricCoordinates)
        {
            if (!ContainsGridIndex(gridIndex))
            {
                barycentricCoordinates = default;
                return false;
            }

            barycentricCoordinates = _values[GetFlatIndex(gridIndex)];
            return barycentricCoordinates.Presence != TripletPresenceFlags.None;
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
        private PartialTriplet<float> CreateOddR(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToOddRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetOddRHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[(vertex + 1) % 6];
            VectorXYInt rightIndex = mainIndex + offsets[vertex];
            return CreatePartial(point, mainIndex, leftIndex, rightIndex, mainCenter, GetOddRHexCenter(leftIndex), GetOddRHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PartialTriplet<float> CreateEvenR(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToEvenRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetEvenRHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[(vertex + 1) % 6];
            VectorXYInt rightIndex = mainIndex + offsets[vertex];
            return CreatePartial(point, mainIndex, leftIndex, rightIndex, mainCenter, GetEvenRHexCenter(leftIndex), GetEvenRHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PartialTriplet<float> CreateOddQ(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToOddQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetOddQHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[vertex];
            VectorXYInt rightIndex = mainIndex + offsets[(vertex + 5) % 6];
            return CreatePartial(point, mainIndex, leftIndex, rightIndex, mainCenter, GetOddQHexCenter(leftIndex), GetOddQHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PartialTriplet<float> CreateEvenQ(PointXY point, VectorXY[] vertices, VectorXYInt[] evenOffsets, VectorXYInt[] oddOffsets)
        {
            VectorXYInt mainIndex = point.ToEvenQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
            VectorXY mainCenter = GetEvenQHexCenter(mainIndex);
            int vertex = GetClosestVertexIndex(point, mainCenter, SourceHexMapGeometry.Radius, vertices, out _);
            VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
            VectorXYInt leftIndex = mainIndex + offsets[vertex];
            VectorXYInt rightIndex = mainIndex + offsets[(vertex + 5) % 6];
            return CreatePartial(point, mainIndex, leftIndex, rightIndex, mainCenter, GetEvenQHexCenter(leftIndex), GetEvenQHexCenter(rightIndex));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PartialTriplet<float> CreatePartial(
            PointXY point,
            VectorXYInt mainIndex,
            VectorXYInt leftIndex,
            VectorXYInt rightIndex,
            VectorXY mainCenter,
            VectorXY leftCenter,
            VectorXY rightCenter)
        {
            Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);
            bool hasMain = ContainsHex(mainIndex);
            bool hasLeft = ContainsHex(leftIndex);
            bool hasRight = ContainsHex(rightIndex);
            return new PartialTriplet<float>(
                hasMain ? barycentric.Main : default,
                hasLeft ? barycentric.Left : default,
                hasRight ? barycentric.Right : default,
                hasMain,
                hasLeft,
                hasRight);
        }

        private bool ContainsHex(VectorXYInt index)
        {
            return (uint)index.X < (uint)SourceHexMapGeometry.Topology.Resolution.X &&
                (uint)index.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
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
