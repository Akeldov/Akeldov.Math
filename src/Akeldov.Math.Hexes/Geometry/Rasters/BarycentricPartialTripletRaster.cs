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
    public sealed class BarycentricPartialTripletRaster : ISpatialRaster<PartialTriplet<float>>
    {
        private readonly PartialTriplet<float>[] _values = Array.Empty<PartialTriplet<float>>();

        /// <summary>
        /// Initializes a new instance that covers the whole source hex map.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        public BarycentricPartialTripletRaster(HexMapGeometry hexMapGeometry)
            : this(hexMapGeometry, hexMapGeometry.ToRasterGeometry(1f))
        {
        }

        /// <summary>
        /// Initializes a clipped barycentric-weight raster over the specified sampling geometry.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public BarycentricPartialTripletRaster(
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
        /// Gets the present barycentric weights at the specified raster coordinates.
        /// </summary>
        /// <param name="x">The horizontal raster coordinate.</param>
        /// <param name="y">The vertical raster coordinate.</param>
        public PartialTriplet<float> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the present barycentric weights at the specified raster coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the raster cell.</param>
        public PartialTriplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)index.X >= (uint)Resolution.X || (uint)index.Y >= (uint)Resolution.Y
                ? throw new ArgumentOutOfRangeException(nameof(index), index, $"Raster index out of bounds: {index}")
                : _values[index.Y * Resolution.X + index.X];
        }

        /// <summary>
        /// Gets the present barycentric weights at the specified flat raster index.
        /// </summary>
        /// <param name="index">The zero-based row-major raster index.</param>
        public PartialTriplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get present barycentric weights at the specified raster coordinates.
        /// </summary>
        /// <param name="gridIndex">The X/Y coordinates of the raster cell.</param>
        /// <param name="barycentricCoordinates">Receives the weights for the present surrounding hexes.</param>
        /// <returns><see langword="true"/> when the cell contains at least one in-bounds weight; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(VectorXYInt gridIndex, out PartialTriplet<float> barycentricCoordinates)
        {
            if ((uint)gridIndex.X >= (uint)Resolution.X || (uint)gridIndex.Y >= (uint)Resolution.Y)
            {
                barycentricCoordinates = default;
                return false;
            }

            barycentricCoordinates = _values[gridIndex.Y * Resolution.X + gridIndex.X];
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
                    throw new InvalidOperationException(
                        $"Unsupported source hex map layout: {SourceHexMapGeometry.Topology.Layout}.");
            }
        }

        private void FillOddR()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.RowLayoutNormalizedHexVertices;
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.RowUnshiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.RowShiftedVectors;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    var point = new PointXY(pointX, pointY);
                    VectorXYInt mainIndex = point.ToOddRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    var mainCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + mainIndex.X * 2f * SourceHexMapGeometry.Apothem + ((mainIndex.Y & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * mainIndex.Y);
                    float minSquaredDistance = float.MaxValue;
                    int closestVertex = 0;
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                    {
                        VectorXY vertexPoint = mainCenter + vertices[vertexIndex] * SourceHexMapGeometry.Radius;
                        float deltaX = point.X - vertexPoint.X;
                        float deltaY = point.Y - vertexPoint.Y;
                        float squaredDistance = deltaX * deltaX + deltaY * deltaY;
                        if (squaredDistance < minSquaredDistance)
                        {
                            minSquaredDistance = squaredDistance;
                            closestVertex = vertexIndex;
                        }
                    }
                    VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt leftIndex = mainIndex + offsets[(closestVertex + 1) % 6];
                    VectorXYInt rightIndex = mainIndex + offsets[closestVertex];
                    var leftCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + leftIndex.X * 2f * SourceHexMapGeometry.Apothem + ((leftIndex.Y & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * leftIndex.Y);
                    var rightCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + rightIndex.X * 2f * SourceHexMapGeometry.Apothem + ((rightIndex.Y & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * rightIndex.Y);
                    Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);
                    bool hasMain = (uint)mainIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)mainIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasLeft = (uint)leftIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)leftIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasRight = (uint)rightIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)rightIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    _values[index] = new PartialTriplet<float>(hasMain ? barycentric.Main : default, hasLeft ? barycentric.Left : default, hasRight ? barycentric.Right : default, hasMain, hasLeft, hasRight);
                }
            }
        }

        private void FillEvenR()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.RowLayoutNormalizedHexVertices;
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.RowShiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.RowUnshiftedVectors;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    var point = new PointXY(pointX, pointY);
                    VectorXYInt mainIndex = point.ToEvenRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    var mainCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + mainIndex.X * 2f * SourceHexMapGeometry.Apothem + ((mainIndex.Y & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * mainIndex.Y);
                    float minSquaredDistance = float.MaxValue;
                    int closestVertex = 0;
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                    {
                        VectorXY vertexPoint = mainCenter + vertices[vertexIndex] * SourceHexMapGeometry.Radius;
                        float deltaX = point.X - vertexPoint.X;
                        float deltaY = point.Y - vertexPoint.Y;
                        float squaredDistance = deltaX * deltaX + deltaY * deltaY;
                        if (squaredDistance < minSquaredDistance)
                        {
                            minSquaredDistance = squaredDistance;
                            closestVertex = vertexIndex;
                        }
                    }
                    VectorXYInt[] offsets = (mainIndex.Y & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt leftIndex = mainIndex + offsets[(closestVertex + 1) % 6];
                    VectorXYInt rightIndex = mainIndex + offsets[closestVertex];
                    var leftCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + leftIndex.X * 2f * SourceHexMapGeometry.Apothem + ((leftIndex.Y & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * leftIndex.Y);
                    var rightCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + rightIndex.X * 2f * SourceHexMapGeometry.Apothem + ((rightIndex.Y & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * rightIndex.Y);
                    Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);
                    bool hasMain = (uint)mainIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)mainIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasLeft = (uint)leftIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)leftIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasRight = (uint)rightIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)rightIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    _values[index] = new PartialTriplet<float>(hasMain ? barycentric.Main : default, hasLeft ? barycentric.Left : default, hasRight ? barycentric.Right : default, hasMain, hasLeft, hasRight);
                }
            }
        }

        private void FillOddQ()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            VectorXYInt[] evenOffsets = BoolExtensions.ColumnUnshiftedEdgeOffsets;
            VectorXYInt[] oddOffsets = BoolExtensions.ColumnShiftedEdgeOffsets;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    var point = new PointXY(pointX, pointY);
                    VectorXYInt mainIndex = point.ToOddQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    var mainCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * mainIndex.X,
                        SourceHexMapGeometry.Origin.Y + mainIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((mainIndex.X & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f));
                    float minSquaredDistance = float.MaxValue;
                    int closestVertex = 0;
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                    {
                        VectorXY vertexPoint = mainCenter + vertices[vertexIndex] * SourceHexMapGeometry.Radius;
                        float deltaX = point.X - vertexPoint.X;
                        float deltaY = point.Y - vertexPoint.Y;
                        float squaredDistance = deltaX * deltaX + deltaY * deltaY;
                        if (squaredDistance < minSquaredDistance)
                        {
                            minSquaredDistance = squaredDistance;
                            closestVertex = vertexIndex;
                        }
                    }
                    VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt leftIndex = mainIndex + offsets[closestVertex];
                    VectorXYInt rightIndex = mainIndex + offsets[(closestVertex + 5) % 6];
                    var leftCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * leftIndex.X,
                        SourceHexMapGeometry.Origin.Y + leftIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((leftIndex.X & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f));
                    var rightCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * rightIndex.X,
                        SourceHexMapGeometry.Origin.Y + rightIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((rightIndex.X & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f));
                    Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);
                    bool hasMain = (uint)mainIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)mainIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasLeft = (uint)leftIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)leftIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasRight = (uint)rightIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)rightIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    _values[index] = new PartialTriplet<float>(hasMain ? barycentric.Main : default, hasLeft ? barycentric.Left : default, hasRight ? barycentric.Right : default, hasMain, hasLeft, hasRight);
                }
            }
        }

        private void FillEvenQ()
        {
            VectorXY[] vertices = Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            VectorXYInt[] evenOffsets = BoolExtensions.ColumnShiftedEdgeOffsets;
            VectorXYInt[] oddOffsets = BoolExtensions.ColumnUnshiftedEdgeOffsets;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    float pointX = Geometry.Origin.X + (x + 0.5f) * cellSize.X;
                    var point = new PointXY(pointX, pointY);
                    VectorXYInt mainIndex = point.ToEvenQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    var mainCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * mainIndex.X,
                        SourceHexMapGeometry.Origin.Y + mainIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((mainIndex.X & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f));
                    float minSquaredDistance = float.MaxValue;
                    int closestVertex = 0;
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                    {
                        VectorXY vertexPoint = mainCenter + vertices[vertexIndex] * SourceHexMapGeometry.Radius;
                        float deltaX = point.X - vertexPoint.X;
                        float deltaY = point.Y - vertexPoint.Y;
                        float squaredDistance = deltaX * deltaX + deltaY * deltaY;
                        if (squaredDistance < minSquaredDistance)
                        {
                            minSquaredDistance = squaredDistance;
                            closestVertex = vertexIndex;
                        }
                    }
                    VectorXYInt[] offsets = (mainIndex.X & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt leftIndex = mainIndex + offsets[closestVertex];
                    VectorXYInt rightIndex = mainIndex + offsets[(closestVertex + 5) % 6];
                    var leftCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * leftIndex.X,
                        SourceHexMapGeometry.Origin.Y + leftIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((leftIndex.X & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f));
                    var rightCenter = new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * rightIndex.X,
                        SourceHexMapGeometry.Origin.Y + rightIndex.Y * 2f * SourceHexMapGeometry.Apothem + ((rightIndex.X & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f));
                    Triplet<float> barycentric = point.BarycentricCoordinates(mainCenter, leftCenter, rightCenter);
                    bool hasMain = (uint)mainIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)mainIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasLeft = (uint)leftIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)leftIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    bool hasRight = (uint)rightIndex.X < (uint)SourceHexMapGeometry.Topology.Resolution.X && (uint)rightIndex.Y < (uint)SourceHexMapGeometry.Topology.Resolution.Y;
                    _values[index] = new PartialTriplet<float>(hasMain ? barycentric.Main : default, hasLeft ? barycentric.Left : default, hasRight ? barycentric.Right : default, hasMain, hasLeft, hasRight);
                }
            }
        }

    }
}
