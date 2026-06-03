using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexAdjacencyGrid
    {
        private const int InvalidHexIndex = -1;
        private const float Sqrt3Over3 = 0.5773502588f;
        private const float OneThird = 0.3333333333f;
        private const float TwoThirds = 0.6666666666f;
        private const float ThreeHalves = 1.5f;

        private IndexedHexAdjacency[] _adjacent;
        private int[] _hexIndices;
        private bool[] _hasHex;

        public HexAdjacencyGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXYInt resolution)
        {
            ThrowIfHexDimensionIsNotPositive(hexWidth, nameof(hexWidth));
            ThrowIfHexDimensionIsNotPositive(hexHeight, nameof(hexHeight));

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            float hexRadius = hexApothem.ConvertHexApothemToRadius();
            GridBounds bounds = GetBounds(hexWidth, hexHeight, layout, hexOrigin, hexApothem, hexRadius);

            Initialize(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                hexApothem,
                hexRadius,
                new VectorXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                resolution,
                null);
        }

        public HexAdjacencyGrid(
            HexAdjacencyMap hexAdjacencyMap,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXYInt resolution)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (hexAdjacencyMap.Width <= 0 || hexAdjacencyMap.Height <= 0)
                throw new ArgumentException("Hex adjacency map dimensions must be positive.", nameof(hexAdjacencyMap));

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            float hexRadius = hexApothem.ConvertHexApothemToRadius();
            GridBounds bounds = GetBounds(
                hexAdjacencyMap.Width,
                hexAdjacencyMap.Height,
                hexAdjacencyMap.Layout,
                hexOrigin,
                hexApothem,
                hexRadius);

            Initialize(
                hexAdjacencyMap.Width,
                hexAdjacencyMap.Height,
                hexAdjacencyMap.Layout,
                hexOrigin,
                hexApothem,
                hexRadius,
                new VectorXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                resolution,
                CreateIndexedAdjacencyCache(hexAdjacencyMap));
        }

        public HexAdjacencyGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            ThrowIfHexDimensionIsNotPositive(hexWidth, nameof(hexWidth));
            ThrowIfHexDimensionIsNotPositive(hexHeight, nameof(hexHeight));

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (!gridOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(gridOrigin), gridOrigin, "Grid origin components must be finite.");

            if (!gridSize.IsFinite || gridSize.X <= 0f || gridSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Grid size components must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            Initialize(
                hexWidth,
                hexHeight,
                layout,
                hexOrigin,
                hexApothem,
                hexApothem.ConvertHexApothemToRadius(),
                gridOrigin,
                gridSize,
                resolution,
                null);
        }

        public HexAdjacencyGrid(
            HexAdjacencyMap hexAdjacencyMap,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (hexAdjacencyMap.Width <= 0 || hexAdjacencyMap.Height <= 0)
                throw new ArgumentException("Hex adjacency map dimensions must be positive.", nameof(hexAdjacencyMap));

            if (!hexOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexOrigin), hexOrigin, "Hex origin components must be finite.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (!gridOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(gridOrigin), gridOrigin, "Grid origin components must be finite.");

            if (!gridSize.IsFinite || gridSize.X <= 0f || gridSize.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Grid size components must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            Initialize(
                hexAdjacencyMap.Width,
                hexAdjacencyMap.Height,
                hexAdjacencyMap.Layout,
                hexOrigin,
                hexApothem,
                hexApothem.ConvertHexApothemToRadius(),
                gridOrigin,
                gridSize,
                resolution,
                CreateIndexedAdjacencyCache(hexAdjacencyMap));
        }

        public VectorXYInt HexResolution { get; private set; }

        public Layout Layout { get; private set; }

        public VectorXY HexOrigin { get; private set; }

        public float HexApothem { get; private set; }

        public float HexRadius { get; private set; }

        public VectorXY Origin { get; private set; }

        public VectorXY Size { get; private set; }

        public VectorXY CellSize { get; private set; }

        public VectorXYInt Resolution { get; private set; }

        public int ResolutionX { get; private set; }

        public int ResolutionY { get; private set; }

        public int Count => _adjacent.Length;

        public IndexedHexAdjacency[] Adjacent => _adjacent;

        public int[] HexIndices => _hexIndices;

        public bool[] HasHex => _hasHex;

        public IndexedHexAdjacency this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _adjacent[GetHitFlatIndex(index)];
        }

        public IndexedHexAdjacency this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfNoHex(index);
                return _adjacent[index];
            }
        }

        public VectorXY GetCellCenter(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return GetCellCenterUnchecked(index.X, index.Y);
        }

        public bool HasHexAt(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return _hasHex[GetFlatIndex(index)];
        }

        public bool TryGetHexIndex(VectorXYInt gridIndex, out VectorXYInt hexIndex)
        {
            if (TryGetHexFlatIndex(gridIndex, out int hexFlatIndex))
            {
                hexIndex = GetHexIndex(hexFlatIndex);
                return true;
            }

            hexIndex = new VectorXYInt(-1, -1);
            return false;
        }

        public VectorXYInt GetHexIndex(VectorXYInt gridIndex)
        {
            return GetHexIndex(GetHexFlatIndex(gridIndex));
        }

        public bool TryGetHexFlatIndex(VectorXYInt gridIndex, out int hexFlatIndex)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = GetFlatIndex(gridIndex);
            hexFlatIndex = _hexIndices[flatIndex];
            return _hasHex[flatIndex];
        }

        public int GetHexFlatIndex(VectorXYInt gridIndex)
        {
            int flatIndex = GetHitFlatIndex(gridIndex);
            return _hexIndices[flatIndex];
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
            VectorXYInt resolution,
            IndexedHexAdjacency[] hexAdjacencyCache)
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
            _adjacent = new IndexedHexAdjacency[checked(resolution.X * resolution.Y)];
            _hexIndices = new int[_adjacent.Length];
            _hasHex = new bool[_adjacent.Length];

            Fill(hexAdjacencyCache ?? CreateIndexedAdjacencyCache(hexWidth, hexHeight, layout));
        }

        private void Fill(IndexedHexAdjacency[] hexAdjacencyCache)
        {
            switch (Layout)
            {
                case Layout.OddR:
                    FillRowLayout(false, hexAdjacencyCache);
                    break;
                case Layout.EvenR:
                    FillRowLayout(true, hexAdjacencyCache);
                    break;
                case Layout.OddQ:
                    FillColumnLayout(false, hexAdjacencyCache);
                    break;
                case Layout.EvenQ:
                    FillColumnLayout(true, hexAdjacencyCache);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Layout));
            }
        }

        private void FillRowLayout(bool evenRowsAreShifted, IndexedHexAdjacency[] hexAdjacencyCache)
        {
            IndexedHexAdjacency[] adjacent = _adjacent;
            int[] hexIndices = _hexIndices;
            bool[] hasHex = _hasHex;
            int resolutionX = ResolutionX;
            int resolutionY = ResolutionY;
            int hexWidth = HexResolution.X;
            int hexHeight = HexResolution.Y;
            float originX = Origin.X;
            float originY = Origin.Y;
            float hexOriginX = HexOrigin.X;
            float hexOriginY = HexOrigin.Y;
            float cellSizeX = CellSize.X;
            float cellSizeY = CellSize.Y;
            float hexRadius = HexRadius;
            var pointyQNumeratorByX = new float[resolutionX];

            for (int x = 0; x < resolutionX; x++)
            {
                float shiftedX = originX + (x + 0.5f) * cellSizeX - hexOriginX;
                pointyQNumeratorByX[x] = Sqrt3Over3 * shiftedX;
            }

            for (int y = 0; y < resolutionY; y++)
            {
                int rowStart = y * resolutionX;
                float shiftedY = originY + (y + 0.5f) * cellSizeY - hexOriginY;
                float qYNumerator = OneThird * shiftedY;
                float r = TwoThirds * shiftedY / hexRadius;

                for (int x = 0; x < resolutionX; x++)
                {
                    float q = (pointyQNumeratorByX[x] - qYNumerator) / hexRadius;
                    RoundPointyTopAxial(q, r, out int qInt, out int rInt);

                    int hexX = evenRowsAreShifted
                        ? qInt + ((rInt + (rInt & 1)) / 2)
                        : qInt + ((rInt - (rInt & 1)) / 2);

                    int flatIndex = rowStart + x;

                    if ((uint)hexX >= (uint)hexWidth || (uint)rInt >= (uint)hexHeight)
                    {
                        hexIndices[flatIndex] = InvalidHexIndex;
                        continue;
                    }

                    int hexIndex = rInt * hexWidth + hexX;
                    hasHex[flatIndex] = true;
                    hexIndices[flatIndex] = hexIndex;
                    adjacent[flatIndex] = hexAdjacencyCache[hexIndex];
                }
            }
        }

        private void FillColumnLayout(bool evenColumnsAreShifted, IndexedHexAdjacency[] hexAdjacencyCache)
        {
            IndexedHexAdjacency[] adjacent = _adjacent;
            int[] hexIndices = _hexIndices;
            bool[] hasHex = _hasHex;
            int resolutionX = ResolutionX;
            int resolutionY = ResolutionY;
            int hexWidth = HexResolution.X;
            int hexHeight = HexResolution.Y;
            float originX = Origin.X;
            float originY = Origin.Y;
            float hexOriginX = HexOrigin.X;
            float hexOriginY = HexOrigin.Y;
            float cellSizeX = CellSize.X;
            float cellSizeY = CellSize.Y;
            float hexRadius = HexRadius;
            var flatQNumeratorByX = new float[resolutionX];
            var flatRNumeratorByX = new float[resolutionX];

            for (int x = 0; x < resolutionX; x++)
            {
                float shiftedX = originX + (x + 0.5f) * cellSizeX - hexOriginX;
                flatQNumeratorByX[x] = TwoThirds * shiftedX;
                flatRNumeratorByX[x] = OneThird * shiftedX;
            }

            for (int y = 0; y < resolutionY; y++)
            {
                int rowStart = y * resolutionX;
                float shiftedY = originY + (y + 0.5f) * cellSizeY - hexOriginY;
                float rYNumerator = Sqrt3Over3 * shiftedY;

                for (int x = 0; x < resolutionX; x++)
                {
                    float q = flatQNumeratorByX[x] / hexRadius;
                    float r = (rYNumerator - flatRNumeratorByX[x]) / hexRadius;
                    RoundFlatTopAxial(q, r, out int qInt, out int rInt);

                    int hexY = evenColumnsAreShifted
                        ? rInt + ((qInt + (qInt & 1)) / 2)
                        : rInt + ((qInt - (qInt & 1)) / 2);

                    int flatIndex = rowStart + x;

                    if ((uint)qInt >= (uint)hexWidth || (uint)hexY >= (uint)hexHeight)
                    {
                        hexIndices[flatIndex] = InvalidHexIndex;
                        continue;
                    }

                    int hexIndex = hexY * hexWidth + qInt;
                    hasHex[flatIndex] = true;
                    hexIndices[flatIndex] = hexIndex;
                    adjacent[flatIndex] = hexAdjacencyCache[hexIndex];
                }
            }
        }

        private static IndexedHexAdjacency[] CreateIndexedAdjacencyCache(HexAdjacencyMap map)
        {
            HexAdjacency[] source = map.Adjacent;
            var cache = new IndexedHexAdjacency[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                HexAdjacency adjacency = source[i];
                cache[i] = new IndexedHexAdjacency(
                    (IndexedHexAdjacencyFlags)adjacency.Flags | IndexedHexAdjacencyFlags.OwnIndex,
                    i,
                    adjacency.Adjacent0Index,
                    adjacency.Adjacent1Index,
                    adjacency.Adjacent2Index,
                    adjacency.Adjacent3Index,
                    adjacency.Adjacent4Index,
                    adjacency.Adjacent5Index);
            }

            return cache;
        }

        private static IndexedHexAdjacency[] CreateIndexedAdjacencyCache(
            int width,
            int height,
            Layout layout)
        {
            var cache = new IndexedHexAdjacency[checked(width * height)];

            switch (layout)
            {
                case Layout.OddR:
                    FillRowLayoutAdjacencyCache(width, height, false, cache);
                    break;
                case Layout.EvenR:
                    FillRowLayoutAdjacencyCache(width, height, true, cache);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutAdjacencyCache(width, height, false, cache);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutAdjacencyCache(width, height, true, cache);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }

            return cache;
        }

        private static void FillRowLayoutAdjacencyCache(
            int width,
            int height,
            bool evenRowsAreShifted,
            IndexedHexAdjacency[] cache)
        {
            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                sbyte[] offsets = HexAdjacencyOffsets.GetRowOffsets(y, evenRowsAreShifted);

                for (int x = 0; x < width; x++)
                {
                    int flatIndex = rowStart + x;
                    cache[flatIndex] = CreateIndexedAdjacency(x, y, flatIndex, offsets, width, height);
                }
            }
        }

        private static void FillColumnLayoutAdjacencyCache(
            int width,
            int height,
            bool evenColumnsAreShifted,
            IndexedHexAdjacency[] cache)
        {
            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    sbyte[] offsets = HexAdjacencyOffsets.GetColumnOffsets(x, evenColumnsAreShifted);
                    int flatIndex = rowStart + x;
                    cache[flatIndex] = CreateIndexedAdjacency(x, y, flatIndex, offsets, width, height);
                }
            }
        }

        private static IndexedHexAdjacency CreateIndexedAdjacency(
            int x,
            int y,
            int flatIndex,
            sbyte[] offsets,
            int width,
            int height)
        {
            var flags = IndexedHexAdjacencyFlags.OwnIndex;

            int adjacent0Index = GetAdjacentFlatIndex(x + offsets[0], y + offsets[1], flatIndex, IndexedHexAdjacencyFlags.Adjacent0, ref flags, width, height);
            int adjacent1Index = GetAdjacentFlatIndex(x + offsets[2], y + offsets[3], flatIndex, IndexedHexAdjacencyFlags.Adjacent1, ref flags, width, height);
            int adjacent2Index = GetAdjacentFlatIndex(x + offsets[4], y + offsets[5], flatIndex, IndexedHexAdjacencyFlags.Adjacent2, ref flags, width, height);
            int adjacent3Index = GetAdjacentFlatIndex(x + offsets[6], y + offsets[7], flatIndex, IndexedHexAdjacencyFlags.Adjacent3, ref flags, width, height);
            int adjacent4Index = GetAdjacentFlatIndex(x + offsets[8], y + offsets[9], flatIndex, IndexedHexAdjacencyFlags.Adjacent4, ref flags, width, height);
            int adjacent5Index = GetAdjacentFlatIndex(x + offsets[10], y + offsets[11], flatIndex, IndexedHexAdjacencyFlags.Adjacent5, ref flags, width, height);

            return new IndexedHexAdjacency(
                flags,
                flatIndex,
                adjacent0Index,
                adjacent1Index,
                adjacent2Index,
                adjacent3Index,
                adjacent4Index,
                adjacent5Index);
        }

        private static int GetAdjacentFlatIndex(
            int x,
            int y,
            int fallbackFlatIndex,
            IndexedHexAdjacencyFlags flag,
            ref IndexedHexAdjacencyFlags flags,
            int width,
            int height)
        {
            if ((uint)x >= (uint)width || (uint)y >= (uint)height)
                return fallbackFlatIndex;

            flags |= flag;
            return y * width + x;
        }

        private VectorXY GetCellCenterUnchecked(int x, int y)
        {
            return new VectorXY(
                Origin.X + (x + 0.5f) * CellSize.X,
                Origin.Y + (y + 0.5f) * CellSize.Y);
        }

        private VectorXYInt GetHexIndex(int hexFlatIndex)
        {
            return new VectorXYInt(
                hexFlatIndex % HexResolution.X,
                hexFlatIndex / HexResolution.X);
        }

        private static void RoundPointyTopAxial(float q, float r, out int qInt, out int rInt)
        {
            float s = -q - r;

            qInt = (int)MathF.Round(q);
            rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);

            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = -rInt - sInt;
            else if (rDiff > sDiff)
                rInt = -qInt - sInt;
        }

        private static void RoundFlatTopAxial(float q, float r, out int qInt, out int rInt)
        {
            float s = -q - r;

            qInt = (int)MathF.Round(q);
            rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);

            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (rDiff > qDiff && rDiff > sDiff)
                rInt = -qInt - sInt;
            else if (qDiff > sDiff)
                qInt = -rInt - sInt;
        }

        private int GetHitFlatIndex(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);

            int flatIndex = GetFlatIndex(index);
            ThrowIfNoHex(flatIndex);
            return flatIndex;
        }

        private void ThrowIfNoHex(int flatIndex)
        {
            if (!_hasHex[flatIndex])
                throw new InvalidOperationException("Grid cell does not hit a hex in the adjacency map.");
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= ResolutionX ||
                index.Y < 0 || index.Y >= ResolutionY)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * ResolutionX + index.X;

        private static GridBounds GetBounds(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return GetOddRowLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.EvenR:
                    return GetEvenRowLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.OddQ:
                    return GetOddColumnLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                case Layout.EvenQ:
                    return GetEvenColumnLayoutBounds(hexWidth, hexHeight, hexOrigin, hexApothem, hexRadius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static GridBounds GetOddRowLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedRowExtra = hexHeight > 1 ? apothem : 0f;
            float halfWidth = Geometry.Constants.Cos30Deg * radius;

            return new GridBounds(
                origin.X - halfWidth,
                origin.Y - radius,
                origin.X + 2f * apothem * (hexWidth - 1) + shiftedRowExtra + halfWidth,
                origin.Y + ThreeHalves * radius * (hexHeight - 1) + radius);
        }

        private static GridBounds GetEvenRowLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedRowExtra = hexHeight > 1 ? apothem : 0f;
            float halfWidth = Geometry.Constants.Cos30Deg * radius;

            return new GridBounds(
                origin.X - shiftedRowExtra - halfWidth,
                origin.Y - radius,
                origin.X + 2f * apothem * (hexWidth - 1) + halfWidth,
                origin.Y + ThreeHalves * radius * (hexHeight - 1) + radius);
        }

        private static GridBounds GetOddColumnLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedColumnExtra = hexWidth > 1 ? apothem : 0f;
            float halfHeight = Geometry.Constants.Sin60Deg * radius;

            return new GridBounds(
                origin.X - radius,
                origin.Y - halfHeight,
                origin.X + ThreeHalves * radius * (hexWidth - 1) + radius,
                origin.Y + 2f * apothem * (hexHeight - 1) + shiftedColumnExtra + halfHeight);
        }

        private static GridBounds GetEvenColumnLayoutBounds(
            int hexWidth,
            int hexHeight,
            VectorXY origin,
            float apothem,
            float radius)
        {
            float shiftedColumnExtra = hexWidth > 1 ? apothem : 0f;
            float halfHeight = Geometry.Constants.Sin60Deg * radius;

            return new GridBounds(
                origin.X - radius,
                origin.Y - shiftedColumnExtra - halfHeight,
                origin.X + ThreeHalves * radius * (hexWidth - 1) + radius,
                origin.Y + 2f * apothem * (hexHeight - 1) + halfHeight);
        }

        private static void ThrowIfHexDimensionIsNotPositive(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Hex grid dimensions must be positive.");
        }

        private readonly struct GridBounds
        {
            public GridBounds(float minX, float minY, float maxX, float maxY)
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
