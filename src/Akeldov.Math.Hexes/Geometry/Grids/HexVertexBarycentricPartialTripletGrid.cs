using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexVertexBarycentricPartialTripletGrid : IGrid<PartialTriplet<float>>
    {
        private const float DefaultHexRadius = 1f;

        private PartialTriplet<float>[] _barycentricCoordinates;

        private int HexWidth { get; set; }

        private int HexHeight { get; set; }

        private VectorXY HexOrigin { get; set; }

        private float HexApothem { get; set; }

        private float HexRadius { get; set; }

        internal VectorXY Origin { get; set; }

        internal VectorXY Size { get; set; }

        private VectorXY CellSize { get; set; }

        public HexVertexBarycentricPartialTripletGrid(
            IndexedHexAdjacencyMap indexedHexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (indexedHexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyMap));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            float apothem = DefaultHexRadius.ConvertHexRadiusToApothem();
            VectorXY gridSize = indexedHexAdjacencyMap.GetBoundingBoxSize(DefaultHexRadius);

            Initialize(
                indexedHexAdjacencyMap.Width,
                indexedHexAdjacencyMap.Height,
                indexedHexAdjacencyMap.Layout,
                GetDefaultHexOrigin(indexedHexAdjacencyMap.Layout, apothem, DefaultHexRadius),
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

        public int Count => _barycentricCoordinates.Length;

        public PartialTriplet<float>[] BarycentricCoordinates => _barycentricCoordinates;

        public int Width => ResolutionX;

        public int Height => ResolutionY;

        public PartialTriplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfGridIndexOutOfBounds(index);
                return _barycentricCoordinates[GetFlatIndex(index)];
            }
        }

        public PartialTriplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _barycentricCoordinates[index];
        }

        public VectorXY GetCellCenter(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return GetCellCenterUnchecked(index.X, index.Y);
        }

        public bool TryGetBarycentricCoordinates(VectorXYInt gridIndex, out PartialTriplet<float> barycentricCoordinates)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = GetFlatIndex(gridIndex);
            barycentricCoordinates = _barycentricCoordinates[flatIndex];
            return barycentricCoordinates.Presence != TripletPresenceFlags.None;
        }

        public PartialTriplet<float> GetBarycentricCoordinates(VectorXYInt gridIndex)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);
            return _barycentricCoordinates[GetFlatIndex(gridIndex)];
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

            _barycentricCoordinates = new PartialTriplet<float>[checked(resolution.X * resolution.Y)];

            Fill();
        }

        private void Fill()
        {
            VectorXY[] normalizedHexVertexes = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertexes(Layout);

            for (int y = 0; y < ResolutionY; y++)
            {
                int rowStart = y * ResolutionX;

                for (int x = 0; x < ResolutionX; x++)
                {
                    int flatIndex = rowStart + x;
                    VectorXY point = GetCellCenterUnchecked(x, y);
                    VectorXYInt mainIndex = point.ToXYIndex(HexRadius, HexOrigin, Layout);
                    _barycentricCoordinates[flatIndex] = CreateBarycentricCoordinates(point, mainIndex, normalizedHexVertexes);
                }
            }
        }

        private PartialTriplet<float> CreateBarycentricCoordinates(
            VectorXY point,
            VectorXYInt mainIndex,
            VectorXY[] normalizedHexVertexes)
        {
            VectorXY mainCenter = GetHexCenter(mainIndex);
            HexVertex nearestVertex = (HexVertex)GetClosestVertexIndex(
                point,
                mainCenter,
                HexRadius,
                normalizedHexVertexes,
                out _);
            Triplet<VectorXYInt> indexTriplet = mainIndex.GetAdjacentTriplet(nearestVertex, Layout);
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
            return (uint)index.X < (uint)HexWidth &&
                (uint)index.Y < (uint)HexHeight;
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= ResolutionX ||
                index.Y < 0 || index.Y >= ResolutionY)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * ResolutionX + index.X;

        private VectorXY GetCellCenterUnchecked(int x, int y)
        {
            return new VectorXY(
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
            VectorXY point,
            VectorXY hexCenter,
            float hexRadius,
            VectorXY[] normalizedHexVertexes,
            out float minSquaredDistance)
        {
            minSquaredDistance = float.MaxValue;
            int closestVertexIndex = 0;

            for (int i = 0; i < normalizedHexVertexes.Length; i++)
            {
                VectorXY vertex = hexCenter + normalizedHexVertexes[i] * hexRadius;
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

        private static float SquaredDistance(VectorXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }
    }
}
