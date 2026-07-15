using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the ChromaticIndexTripletGrid type.
    /// </summary>
    public sealed class ChromaticIndexTripletGrid : ISpatialRaster<Triplet<byte>>
    {
        private Triplet<byte>[] _values = Array.Empty<Triplet<byte>>();

        /// <summary>
        /// Initializes a new instance of the ChromaticIndexTripletGrid type.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public ChromaticIndexTripletGrid(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;

            _values = new Triplet<byte>[checked(Resolution.X * Resolution.Y)];

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
        public Triplet<byte> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Triplet<byte> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index.X >= (uint)Resolution.X ||
                    (uint)index.Y >= (uint)Resolution.Y)
                    throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");

                return _values[index.Y * Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Triplet<byte> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get a value at the specified index.
        /// </summary>
        /// <param name="gridIndex">The gridIndex value.</param>
        /// <param name="chromaticIndices">The chromaticIndices value.</param>
        public bool TryGetValue(VectorXYInt gridIndex, out Triplet<byte> chromaticIndices)
        {
            if ((uint)gridIndex.X >= (uint)Resolution.X ||
                (uint)gridIndex.Y >= (uint)Resolution.Y)
            {
                chromaticIndices = default;
                return false;
            }

            int flatIndex = gridIndex.Y * Resolution.X + gridIndex.X;
            chromaticIndices = _values[flatIndex];
            return true;
        }

        private void Fill()
        {
            switch (SourceHexMapGeometry.Topology.Layout)
            {
                case Layout.OddR: FillOddR(); break;
                case Layout.EvenR: FillEvenR(); break;
                case Layout.OddQ: FillOddQ(); break;
                case Layout.EvenQ: FillEvenQ(); break;
                default: throw new ArgumentOutOfRangeException(nameof(SourceHexMapGeometry.Topology.Layout));
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
                    _values[index] = new Triplet<byte>(
                        (byte)mainIndex.GetOddRChromaticClass(),
                        (byte)leftIndex.GetOddRChromaticClass(),
                        (byte)rightIndex.GetOddRChromaticClass());
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
                    _values[index] = new Triplet<byte>(
                        (byte)mainIndex.GetEvenRChromaticClass(),
                        (byte)leftIndex.GetEvenRChromaticClass(),
                        (byte)rightIndex.GetEvenRChromaticClass());
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
                    _values[index] = new Triplet<byte>(
                        (byte)mainIndex.GetOddQChromaticClass(),
                        (byte)leftIndex.GetOddQChromaticClass(),
                        (byte)rightIndex.GetOddQChromaticClass());
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
                    _values[index] = new Triplet<byte>(
                        (byte)mainIndex.GetEvenQChromaticClass(),
                        (byte)leftIndex.GetEvenQChromaticClass(),
                        (byte)rightIndex.GetEvenQChromaticClass());
                }
            }
        }

    }
}
