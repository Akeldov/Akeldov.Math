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
            Layout layout = SourceHexMapGeometry.Topology.Layout;
            VectorXY[] normalizedHexVertices = Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(layout);

            for (int y = 0; y < Resolution.Y; y++)
            {
                int rowStart = y * Resolution.X;

                for (int x = 0; x < Resolution.X; x++)
                {
                    int flatIndex = rowStart + x;
                    PointXY point = Geometry.GetCellCenter(x, y);
                    VectorXYInt mainIndex = point.ToXYIndex(
                        SourceHexMapGeometry.Radius,
                        SourceHexMapGeometry.Origin,
                        layout);
                    _values[flatIndex] = CreateBarycentricCoordinates(
                        point,
                        mainIndex,
                        normalizedHexVertices);
                }
            }
        }

        private PartialTriplet<float> CreateBarycentricCoordinates(
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
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(
                nearestVertex,
                SourceHexMapGeometry.Topology.Layout);
            Triplet<float> barycentricCoordinates = point.BarycentricCoordinates(
                mainCenter,
                GetHexCenter(indexTriplet.Left),
                GetHexCenter(indexTriplet.Right));

            float main = default;
            float left = default;
            float right = default;
            TripletPresenceFlags presence = TripletPresenceFlags.None;

            if (ContainsHex(indexTriplet.Main))
            {
                main = barycentricCoordinates.Main;
                presence |= TripletPresenceFlags.Main;
            }

            if (ContainsHex(indexTriplet.Left))
            {
                left = barycentricCoordinates.Left;
                presence |= TripletPresenceFlags.Left;
            }

            if (ContainsHex(indexTriplet.Right))
            {
                right = barycentricCoordinates.Right;
                presence |= TripletPresenceFlags.Right;
            }

            return new PartialTriplet<float>(main, left, right, presence);
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

        private VectorXY GetHexCenter(VectorXYInt index)
        {
            HexMapGeometry geometry = SourceHexMapGeometry;
            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        geometry.Origin.X + index.X * 2f * geometry.Apothem + ((index.Y & 1) == 1 ? geometry.Apothem : 0f),
                        geometry.Origin.Y + 1.5f * geometry.Radius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        geometry.Origin.X + index.X * 2f * geometry.Apothem + ((index.Y & 1) == 1 ? -geometry.Apothem : 0f),
                        geometry.Origin.Y + 1.5f * geometry.Radius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        geometry.Origin.X + 1.5f * geometry.Radius * index.X,
                        geometry.Origin.Y + index.Y * 2f * geometry.Apothem + ((index.X & 1) == 1 ? geometry.Apothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        geometry.Origin.X + 1.5f * geometry.Radius * index.X,
                        geometry.Origin.Y + index.Y * 2f * geometry.Apothem + ((index.X & 1) == 1 ? -geometry.Apothem : 0f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry.Topology.Layout));
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
