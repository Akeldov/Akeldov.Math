using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class IndexPartialTripletGrid : IGrid<PartialTriplet<VectorXYInt>>
    {
        private const float DefaultHexRadius = 1f;

        private PartialTriplet<VectorXYInt>[] _values = Array.Empty<PartialTriplet<VectorXYInt>>();

        private int HexWidth { get; set; }

        private int HexHeight { get; set; }

        private VectorXY HexOrigin { get; set; }

        private float HexApothem { get; set; }

        private float HexRadius { get; set; }

        internal VectorXY Origin { get; set; }

        internal VectorXY Size { get; set; }

        private VectorXY CellSize { get; set; }

        public IndexPartialTripletGrid(
            IndexSeptupletMap hexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            float apothem = DefaultHexRadius.ConvertHexRadiusToApothem();
            VectorXY gridSize = hexAdjacencyMap.GetBoundingBoxSize(DefaultHexRadius);

            Initialize(
                hexAdjacencyMap.Width,
                hexAdjacencyMap.Height,
                hexAdjacencyMap.Layout,
                GetDefaultHexOrigin(hexAdjacencyMap.Layout, apothem, DefaultHexRadius),
                apothem,
                DefaultHexRadius,
                VectorXY.Zero,
                gridSize,
                resolution);
        }

        public VectorXYInt HexResolution { get; private set; }

        public Layout Layout { get; private set; }

        public VectorXYInt Resolution { get; private set; }

        public int ResolutionX { get; private set; }

        public int ResolutionY { get; private set; }

        public int Count => _values.Length;

        public int Width => ResolutionX;

        public int Height => ResolutionY;

        public PartialTriplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfGridIndexOutOfBounds(index);
                return _values[GetFlatIndex(index)];
            }
        }

        public PartialTriplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        public bool TryGetValue(VectorXYInt gridIndex, out PartialTriplet<VectorXYInt> indexTriplet)
        {
            if (!ContainsGridIndex(gridIndex))
            {
                indexTriplet = default;
                return false;
            }

            int flatIndex = GetFlatIndex(gridIndex);
            indexTriplet = _values[flatIndex];
            return indexTriplet.Presence != TripletPresenceFlags.None;
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
            HexWidth = hexWidth;
            HexHeight = hexHeight;
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

            _values = new PartialTriplet<VectorXYInt>[checked(resolution.X * resolution.Y)];

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
                    _values[flatIndex] = CreateIndexTriplet(point, mainIndex, normalizedHexVertices);
                }
            }
        }

        private PartialTriplet<VectorXYInt> CreateIndexTriplet(
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

            VectorXYInt main = default;
            VectorXYInt left = default;
            VectorXYInt right = default;
            TripletPresenceFlags presence = TripletPresenceFlags.None;

            if (ContainsHex(indexTriplet.Main))
            {
                main = indexTriplet.Main;
                presence |= TripletPresenceFlags.Main;
            }

            if (ContainsHex(indexTriplet.Left))
            {
                left = indexTriplet.Left;
                presence |= TripletPresenceFlags.Left;
            }

            if (ContainsHex(indexTriplet.Right))
            {
                right = indexTriplet.Right;
                presence |= TripletPresenceFlags.Right;
            }

            return new PartialTriplet<VectorXYInt>(main, left, right, presence);
        }

        private bool ContainsHex(VectorXYInt index)
        {
            return (uint)index.X < (uint)HexWidth &&
                (uint)index.Y < (uint)HexHeight;
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

        private static VectorXY GetDefaultHexOrigin(Layout layout, float apothem, float radius)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(apothem, radius);
                case Layout.EvenR:
                    return new VectorXY(2f * apothem, radius);
                case Layout.OddQ:
                    return new VectorXY(radius, apothem);
                case Layout.EvenQ:
                    return new VectorXY(radius, 2f * apothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static void ValidateGridParameters(
            int hexWidth,
            int hexHeight,
            VectorXY hexOrigin,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            if (hexWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexWidth), hexWidth, "Hex grid dimensions must be positive.");

            if (hexHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexHeight), hexHeight, "Hex grid dimensions must be positive.");

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (!gridOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(gridOrigin), gridOrigin, "Grid origin components must be finite.");

            if (!gridSize.IsFinite || gridSize.X <= 0f || gridSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Grid size components must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");
        }

        private static float SquaredDistance(PointXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }
    }
}
