using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the BarycentricTripletGrid type.
    /// </summary>
    public sealed class BarycentricTripletGrid : IRaster<Triplet<float>>
    {
        private Triplet<float>[] _values = Array.Empty<Triplet<float>>();

        private VectorXY HexOrigin { get; set; }

        private float HexApothem { get; set; }

        private float HexRadius { get; set; }

        internal VectorXY Origin { get; set; }

        internal VectorXY Size { get; set; }

        private VectorXY CellSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the BarycentricTripletGrid type.
        /// </summary>
        /// <param name="geometry">The hex map geometry.</param>
        /// <param name="resolution">The Resolution value.</param>
        public BarycentricTripletGrid(
            HexMapGeometry geometry,
            VectorXYInt resolution)
        {
            if (geometry.Topology.Resolution.X <= 0 || geometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map dimensions must be positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            var bounds = geometry.GetBoundingBox();
            VectorXY gridOrigin = geometry.Topology.Layout switch
            {
                Layout.EvenR when geometry.Topology.Resolution.Y > 1 => new VectorXY(
                    geometry.Origin.X - geometry.Apothem - Geometry.Constants.Cos30Deg * geometry.Radius,
                    bounds.Min.Y),
                Layout.EvenQ when geometry.Topology.Resolution.X > 1 => new VectorXY(
                    bounds.Min.X,
                    geometry.Origin.Y - geometry.Apothem - Geometry.Constants.Sin60Deg * geometry.Radius),
                _ => new VectorXY(bounds.Min.X, bounds.Min.Y),
            };
            VectorXY gridSize = geometry.Topology.Layout switch
            {
                Layout.EvenR when geometry.Topology.Resolution.Y > 1 => new VectorXY(
                    2f * geometry.Apothem * (geometry.Topology.Resolution.X - 1) + geometry.Apothem + 2f * Geometry.Constants.Cos30Deg * geometry.Radius,
                    bounds.Height),
                Layout.EvenQ when geometry.Topology.Resolution.X > 1 => new VectorXY(
                    bounds.Width,
                    2f * geometry.Apothem * (geometry.Topology.Resolution.Y - 1) + geometry.Apothem + 2f * Geometry.Constants.Sin60Deg * geometry.Radius),
                _ => bounds.Size,
            };

            Initialize(
                geometry.Topology.Resolution.X,
                geometry.Topology.Resolution.Y,
                geometry.Topology.Layout,
                geometry.Origin,
                geometry.Apothem,
                geometry.Radius,
                gridOrigin,
                gridSize,
                resolution);
        }

        /// <summary>
        /// Gets the HexResolution value.
        /// </summary>
        public VectorXYInt HexResolution { get; private set; }

        /// <summary>
        /// Gets the Layout value.
        /// </summary>
        public Layout Layout { get; private set; }

        /// <summary>
        /// Gets the Resolution value.
        /// </summary>
        public VectorXYInt Resolution { get; private set; }

        /// <summary>
        /// Gets the ResolutionX value.
        /// </summary>
        public int ResolutionX { get; private set; }

        /// <summary>
        /// Gets the ResolutionY value.
        /// </summary>
        public int ResolutionY { get; private set; }

        /// <summary>
        /// Gets the Count value.
        /// </summary>
        public int Count => _values.Length;

        /// <summary>
        /// Gets the Width value.
        /// </summary>
        public int Width => ResolutionX;

        /// <summary>
        /// Gets the Height value.
        /// </summary>
        public int Height => ResolutionY;

        /// <summary>
        /// Gets the value at the specified grid coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public Triplet<float> this[int x, int y] => _values[y * ResolutionX + x];

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

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="gridIndex">The gridIndex value.</param>
        public Triplet<float> GetBarycentricCoordinates(VectorXYInt gridIndex)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);
            return _values[GetFlatIndex(gridIndex)];
        }

        private void Initialize(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            ValidateGridParameters(hexWidth, hexHeight, hexOrigin, gridOrigin, gridSize, resolution);

            HexResolution = new VectorXYInt(hexWidth, hexHeight);
            Layout = layout;
            HexOrigin = hexOrigin;
            HexApothem = hexApothem;
            HexRadius = hexRadius;
            Origin = gridOrigin;
            Size = gridSize;
            CellSize = new VectorXY(gridSize.X / resolution.X, gridSize.Y / resolution.Y);
            Resolution = resolution;
            ResolutionX = resolution.X;
            ResolutionY = resolution.Y;

            _values = new Triplet<float>[checked(resolution.X * resolution.Y)];

            Fill();
        }

        private void Fill()
        {
            VectorXY[] normalizedHexVertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Layout);

            for (int y = 0; y < ResolutionY; y++)
            {
                int rowStart = y * ResolutionX;

                for (int x = 0; x < ResolutionX; x++)
                {
                    int flatIndex = rowStart + x;
                    PointXY point = GetCellCenterUnchecked(x, y);
                    VectorXYInt mainIndex = point.ToXYIndex(HexRadius, HexOrigin, Layout);
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
                HexRadius,
                normalizedHexVertices,
                out _);
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(nearestVertex, Layout);

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
            return (uint)index.X < (uint)ResolutionX &&
                (uint)index.Y < (uint)ResolutionY;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * ResolutionX + index.X;

        private PointXY GetCellCenterUnchecked(int x, int y)
        {
            return new PointXY(
                Origin.X + (x + 0.5f) * CellSize.X,
                Origin.Y + (y + 0.5f) * CellSize.Y);
        }

        private VectorXY GetHexCenter(VectorXYInt index)
        {
            switch (Layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        HexOrigin.X + index.X * 2f * HexApothem + ((index.Y & 1) == 1 ? HexApothem : 0f),
                        HexOrigin.Y + 1.5f * HexRadius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        HexOrigin.X + index.X * 2f * HexApothem + ((index.Y & 1) == 1 ? -HexApothem : 0f),
                        HexOrigin.Y + 1.5f * HexRadius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        HexOrigin.X + 1.5f * HexRadius * index.X,
                        HexOrigin.Y + index.Y * 2f * HexApothem + ((index.X & 1) == 1 ? HexApothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        HexOrigin.X + 1.5f * HexRadius * index.X,
                        HexOrigin.Y + index.Y * 2f * HexApothem + ((index.X & 1) == 1 ? -HexApothem : 0f));
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

        private static void ValidateHexGrid(
            int hexWidth,
            int hexHeight,
            VectorXY hexOrigin,
            VectorXYInt resolution)
        {
            if (hexWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexWidth), hexWidth, "Hex grid dimensions must be positive.");

            if (hexHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexHeight), hexHeight, "Hex grid dimensions must be positive.");

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");
        }

        private static void ValidateGridParameters(
            int hexWidth,
            int hexHeight,
            VectorXY hexOrigin,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            ValidateHexGrid(hexWidth, hexHeight, hexOrigin, resolution);

            if (!gridOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(gridOrigin), gridOrigin, "Grid origin components must be finite.");

            if (!gridSize.IsFinite || gridSize.X <= 0f || gridSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Grid size components must be finite and positive.");
        }

        private static float SquaredDistance(PointXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }

    }
}
