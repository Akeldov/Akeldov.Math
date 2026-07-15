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
            VectorXY[] normalizedHexVertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(SourceHexMapGeometry.Topology.Layout);

            for (int y = 0; y < Resolution.Y; y++)
            {
                int rowStart = y * Resolution.X;

                for (int x = 0; x < Resolution.X; x++)
                {
                    int flatIndex = rowStart + x;
                    PointXY point = Geometry.GetCellCenter(x, y);
                    VectorXYInt mainIndex = point.ToXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin, SourceHexMapGeometry.Topology.Layout);
                    _values[flatIndex] = CreateBarycentricCoordinates(point, mainIndex, normalizedHexVertices);
                }
            }
        }

        private Triplet<float> CreateBarycentricCoordinates(
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
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(nearestVertex, SourceHexMapGeometry.Topology.Layout);

            return point.BarycentricCoordinates(
                mainCenter,
                GetHexCenter(indexTriplet.Left),
                GetHexCenter(indexTriplet.Right));
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
            switch (SourceHexMapGeometry.Topology.Layout)
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
