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
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout => SourceHexMapGeometry.Topology.Layout;

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
                ThrowIfGridIndexOutOfBounds(index);
                return _values[GetFlatIndex(index)];
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
            if (!ContainsGridIndex(gridIndex))
            {
                chromaticIndices = default;
                return false;
            }

            int flatIndex = GetFlatIndex(gridIndex);
            chromaticIndices = _values[flatIndex];
            return true;
        }

        private void Fill()
        {
            switch (Layout)
            {
                case Layout.OddR: FillOddR(); break;
                case Layout.EvenR: FillEvenR(); break;
                case Layout.OddQ: FillOddQ(); break;
                case Layout.EvenQ: FillEvenQ(); break;
                default: throw new ArgumentOutOfRangeException(nameof(Layout));
            }
        }

        private void FillOddR()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.RowLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * Geometry.CellSize.X, Geometry.Origin.Y + (y + 0.5f) * Geometry.CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToOddRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin), vertices);
            }
        }

        private void FillEvenR()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.RowLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * Geometry.CellSize.X, Geometry.Origin.Y + (y + 0.5f) * Geometry.CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToEvenRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin), vertices);
            }
        }

        private void FillOddQ()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * Geometry.CellSize.X, Geometry.Origin.Y + (y + 0.5f) * Geometry.CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToOddQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin), vertices);
            }
        }

        private void FillEvenQ()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * Geometry.CellSize.X, Geometry.Origin.Y + (y + 0.5f) * Geometry.CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToEvenQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin), vertices);
            }
        }

        private Triplet<byte> CreateChromaticIndices(
            PointXY point,
            VectorXYInt mainIndex,
            VectorXY[] normalizedHexVertices)
        {
            VectorXY mainCenter = GetHexCenter(mainIndex);
            HexVertex nearestVertex = (HexVertex)GetClosestVertexIndex(
                point,
                mainCenter,
                SourceHexMapGeometry.Radius,
                normalizedHexVertices,
                out _);
            return mainIndex.GetAdjacentTriplet(nearestVertex, Layout).GetChromaticTriplet(Layout);
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

        private VectorXY GetHexCenter(VectorXYInt index)
        {
            switch (Layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        SourceHexMapGeometry.Origin.X + index.X * 2f * SourceHexMapGeometry.Apothem + ((index.Y & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        SourceHexMapGeometry.Origin.X + index.X * 2f * SourceHexMapGeometry.Apothem + ((index.Y & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f),
                        SourceHexMapGeometry.Origin.Y + 1.5f * SourceHexMapGeometry.Radius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * index.X,
                        SourceHexMapGeometry.Origin.Y + index.Y * 2f * SourceHexMapGeometry.Apothem + ((index.X & 1) == 1 ? SourceHexMapGeometry.Apothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        SourceHexMapGeometry.Origin.X + 1.5f * SourceHexMapGeometry.Radius * index.X,
                        SourceHexMapGeometry.Origin.Y + index.Y * 2f * SourceHexMapGeometry.Apothem + ((index.X & 1) == 1 ? -SourceHexMapGeometry.Apothem : 0f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(Layout));
            }
        }

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
