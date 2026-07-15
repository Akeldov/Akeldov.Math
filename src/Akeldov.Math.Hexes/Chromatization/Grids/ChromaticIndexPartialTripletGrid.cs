using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the ChromaticIndexPartialTripletGrid type.
    /// </summary>
    public sealed class ChromaticIndexPartialTripletGrid : IRaster<PartialTriplet<byte>>
    {
        private const float DefaultHexRadius = 1f;

        private PartialTriplet<byte>[] _values = Array.Empty<PartialTriplet<byte>>();

        private int HexWidth { get; set; }

        private int HexHeight { get; set; }

        private VectorXY HexOrigin { get; set; }

        private float HexApothem { get; set; }

        private float HexRadius { get; set; }

        internal VectorXY Origin { get; set; }

        internal VectorXY Size { get; set; }

        private VectorXY CellSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChromaticIndexPartialTripletGrid type.
        /// </summary>
        /// <param name="hexAdjacencyMap">The HexAdjacencyMap value.</param>
        /// <param name="resolution">The Resolution value.</param>
        public ChromaticIndexPartialTripletGrid(
            IndexSeptupletMap hexAdjacencyMap,
            VectorXYInt resolution)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "Grid resolution components must be positive.");

            float apothem = DefaultHexRadius.ConvertHexRadiusToApothem();
            var geometry = new HexMapGeometry(hexAdjacencyMap.Topology.Resolution.X, hexAdjacencyMap.Topology.Resolution.Y, DefaultHexRadius, hexAdjacencyMap.Topology.Layout);
            VectorXY gridSize = geometry.GetBoundingBoxSize();

            Initialize(
                hexAdjacencyMap.Topology.Resolution.X,
                hexAdjacencyMap.Topology.Resolution.Y,
                hexAdjacencyMap.Topology.Layout,
                GetDefaultHexOrigin(hexAdjacencyMap.Topology.Layout, apothem, DefaultHexRadius),
                apothem,
                DefaultHexRadius,
                VectorXY.Zero,
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
        /// Gets the value at the specified grid coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public PartialTriplet<byte> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public PartialTriplet<byte> this[VectorXYInt index]
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
        public PartialTriplet<byte> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
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

            _values = new PartialTriplet<byte>[checked(resolution.X * resolution.Y)];

            Fill();
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
                var point = new PointXY(Origin.X + (x + 0.5f) * CellSize.X, Origin.Y + (y + 0.5f) * CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToOddRXYIndex(HexRadius, HexOrigin), vertices);
            }
        }

        private void FillEvenR()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.RowLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Origin.X + (x + 0.5f) * CellSize.X, Origin.Y + (y + 0.5f) * CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToEvenRXYIndex(HexRadius, HexOrigin), vertices);
            }
        }

        private void FillOddQ()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Origin.X + (x + 0.5f) * CellSize.X, Origin.Y + (y + 0.5f) * CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToOddQXYIndex(HexRadius, HexOrigin), vertices);
            }
        }

        private void FillEvenQ()
        {
            VectorXY[] vertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.ColumnLayoutNormalizedHexVertices;
            for (int index = 0, y = 0; y < Resolution.Y; y++)
            for (int x = 0; x < Resolution.X; x++, index++)
            {
                var point = new PointXY(Origin.X + (x + 0.5f) * CellSize.X, Origin.Y + (y + 0.5f) * CellSize.Y);
                _values[index] = CreateChromaticIndices(point, point.ToEvenQXYIndex(HexRadius, HexOrigin), vertices);
            }
        }

        private PartialTriplet<byte> CreateChromaticIndices(
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

            byte main = default;
            byte left = default;
            byte right = default;
            TripletPresenceFlags presence = TripletPresenceFlags.None;

            if (ContainsHex(indexTriplet.Main))
            {
                main = (byte)indexTriplet.Main.GetChromaticClass(Layout);
                presence |= TripletPresenceFlags.Main;
            }

            if (ContainsHex(indexTriplet.Left))
            {
                left = (byte)indexTriplet.Left.GetChromaticClass(Layout);
                presence |= TripletPresenceFlags.Left;
            }

            if (ContainsHex(indexTriplet.Right))
            {
                right = (byte)indexTriplet.Right.GetChromaticClass(Layout);
                presence |= TripletPresenceFlags.Right;
            }

            return new PartialTriplet<byte>(main, left, right, presence);
        }

        private bool ContainsHex(VectorXYInt index)
        {
            return (uint)index.X < (uint)HexWidth &&
                (uint)index.Y < (uint)HexHeight;
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= Resolution.X ||
                index.Y < 0 || index.Y >= Resolution.Y)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Resolution.X + index.X;

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
