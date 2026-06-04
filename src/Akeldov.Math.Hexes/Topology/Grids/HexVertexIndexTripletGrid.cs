using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology.Maps.BoundingBox;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexVertexIndexTripletGrid : IGrid<Triplet<VectorXYInt>>
    {
        private const float DefaultHexRadius = 1f;

        private Triplet<VectorXYInt>[] _indexTriplets;

        private VectorXY HexOrigin { get; set; }

        private float HexApothem { get; set; }

        private float HexRadius { get; set; }

        internal VectorXY Origin { get; set; }

        internal VectorXY Size { get; set; }

        private VectorXY CellSize { get; set; }

        public HexVertexIndexTripletGrid(
            IndexedHexAdjacencyMap indexedHexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (indexedHexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyMap));

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

        public HexVertexIndexTripletGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            VectorXYInt resolution)
        {
            ValidateHexGrid(hexWidth, hexHeight, hexOrigin, resolution);

            float hexApothem = DefaultHexRadius.ConvertHexRadiusToApothem();
            Bounds bounds = GetBounds(hexWidth, hexHeight, layout, hexOrigin, hexApothem, DefaultHexRadius);

            Initialize(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                hexApothem,
                DefaultHexRadius,
                new VectorXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                resolution);
        }

        public HexVertexIndexTripletGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
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

            Initialize(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                DefaultHexRadius.ConvertHexRadiusToApothem(),
                DefaultHexRadius,
                gridOrigin,
                gridSize,
                resolution);
        }

        public VectorXYInt HexResolution { get; private set; }

        public Layout Layout { get; private set; }

        public VectorXYInt Resolution { get; private set; }

        public int ResolutionX { get; private set; }

        public int ResolutionY { get; private set; }

        public int Count => _indexTriplets.Length;

        public Triplet<VectorXYInt>[] IndexTriplets => _indexTriplets;

        public int Width => ResolutionX;

        public int Height => ResolutionY;

        public Triplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfGridIndexOutOfBounds(index);
                return _indexTriplets[GetFlatIndex(index)];
            }
        }

        public Triplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indexTriplets[index];
        }

        public VectorXY GetCellCenter(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return GetCellCenterUnchecked(index.X, index.Y);
        }

        public bool TryGetIndexTriplet(VectorXYInt gridIndex, out Triplet<VectorXYInt> indexTriplet)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = GetFlatIndex(gridIndex);
            indexTriplet = _indexTriplets[flatIndex];
            return true;
        }

        public Triplet<VectorXYInt> GetIndexTriplet(VectorXYInt gridIndex)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);
            return _indexTriplets[GetFlatIndex(gridIndex)];
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

            _indexTriplets = new Triplet<VectorXYInt>[checked(resolution.X * resolution.Y)];

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
                    _indexTriplets[flatIndex] = CreateIndexTriplet(point, mainIndex, normalizedHexVertexes);
                }
            }
        }

        private Triplet<VectorXYInt> CreateIndexTriplet(
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
            return mainIndex.GetAdjacentTriplet(nearestVertex, Layout);
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

        private static Bounds GetBounds(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY origin,
            float apothem,
            float radius)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new Bounds(
                        origin.X - Geometry.Constants.Cos30Deg * radius,
                        origin.Y - radius,
                        origin.X + 2f * apothem * (hexWidth - 1) + (hexHeight > 1 ? apothem : 0f) + Geometry.Constants.Cos30Deg * radius,
                        origin.Y + 1.5f * radius * (hexHeight - 1) + radius);
                case Layout.EvenR:
                    return new Bounds(
                        origin.X - (hexHeight > 1 ? apothem : 0f) - Geometry.Constants.Cos30Deg * radius,
                        origin.Y - radius,
                        origin.X + 2f * apothem * (hexWidth - 1) + Geometry.Constants.Cos30Deg * radius,
                        origin.Y + 1.5f * radius * (hexHeight - 1) + radius);
                case Layout.OddQ:
                    return new Bounds(
                        origin.X - radius,
                        origin.Y - Geometry.Constants.Sin60Deg * radius,
                        origin.X + 1.5f * radius * (hexWidth - 1) + radius,
                        origin.Y + 2f * apothem * (hexHeight - 1) + (hexWidth > 1 ? apothem : 0f) + Geometry.Constants.Sin60Deg * radius);
                case Layout.EvenQ:
                    return new Bounds(
                        origin.X - radius,
                        origin.Y - (hexWidth > 1 ? apothem : 0f) - Geometry.Constants.Sin60Deg * radius,
                        origin.X + 1.5f * radius * (hexWidth - 1) + radius,
                        origin.Y + 2f * apothem * (hexHeight - 1) + Geometry.Constants.Sin60Deg * radius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
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

        private static float SquaredDistance(VectorXY left, VectorXY right)
        {
            float x = left.X - right.X;
            float y = left.Y - right.Y;
            return x * x + y * y;
        }

        private readonly struct Bounds
        {
            public Bounds(float minX, float minY, float maxX, float maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public float MinX { get; }

            public float MinY { get; }

            public float MaxX { get; }

            public float MaxY { get; }

            public float Width => MaxX - MinX;

            public float Height => MaxY - MinY;
        }
    }
}
