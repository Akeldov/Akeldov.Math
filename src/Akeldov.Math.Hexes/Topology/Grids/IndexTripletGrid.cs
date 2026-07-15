using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the IndexTripletGrid type.
    /// </summary>
    public sealed class IndexTripletGrid : IRaster<Triplet<VectorXYInt>>
    {
        private Triplet<VectorXYInt>[] _values = Array.Empty<Triplet<VectorXYInt>>();

        /// <summary>
        /// Initializes a new instance of the IndexTripletGrid type.
        /// </summary>
        /// <param name="topology">The hex map topology.</param>
        /// <param name="resolution">The Resolution value.</param>
        public IndexTripletGrid(HexMapTopology topology, VectorXYInt resolution)
        {
            const float hexRadius = 1f;
            float hexApothem = hexRadius.ConvertHexRadiusToApothem();
            VectorXY hexOrigin = GetDefaultHexOrigin(topology.Layout, hexApothem, hexRadius);
            var geometry = new HexMapGeometry(topology.Resolution.X, topology.Resolution.Y, hexRadius, topology.Layout);
            VectorXY gridSize = geometry.GetBoundingBoxSize();
            VectorXY cellSize = new VectorXY(gridSize.X / resolution.X, gridSize.Y / resolution.Y);

            ValidateGridParameters(
                topology.Resolution.X,
                topology.Resolution.Y,
                hexOrigin,
                VectorXY.Zero,
                gridSize,
                resolution);

            Topology = topology;
            Resolution = resolution;
            _values = new Triplet<VectorXYInt>[checked(resolution.X * resolution.Y)];

            Fill(hexOrigin, hexApothem, hexRadius, VectorXY.Zero, cellSize);
        }

        /// <summary>
        /// Gets the hex map topology.
        /// </summary>
        public HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the Resolution value.
        /// </summary>
        public VectorXYInt Resolution { get; private set; }

        /// <summary>
        /// Gets the value at the specified grid coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public Triplet<VectorXYInt> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Triplet<VectorXYInt> this[VectorXYInt index]
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
        public Triplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get a value at the specified index.
        /// </summary>
        /// <param name="gridIndex">The gridIndex value.</param>
        /// <param name="indexTriplet">The indexTriplet value.</param>
        public bool TryGetValue(VectorXYInt gridIndex, out Triplet<VectorXYInt> indexTriplet)
        {
            if (!ContainsGridIndex(gridIndex))
            {
                indexTriplet = default;
                return false;
            }

            int flatIndex = GetFlatIndex(gridIndex);
            indexTriplet = _values[flatIndex];
            return true;
        }

        private void Fill(
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius,
            VectorXY gridOrigin,
            VectorXY cellSize)
        {
            VectorXY[] normalizedHexVertices = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetNormalizedHexVertices(Topology.Layout);

            for (int y = 0; y < Resolution.Y; y++)
            {
                int rowStart = y * Resolution.X;

                for (int x = 0; x < Resolution.X; x++)
                {
                    int flatIndex = rowStart + x;
                    PointXY point = new PointXY(
                        gridOrigin.X + (x + 0.5f) * cellSize.X,
                        gridOrigin.Y + (y + 0.5f) * cellSize.Y);
                    VectorXYInt mainIndex = point.ToXYIndex(hexRadius, hexOrigin, Topology.Layout);
                    _values[flatIndex] = CreateIndexTriplet(
                        point,
                        mainIndex,
                        normalizedHexVertices,
                        hexOrigin,
                        hexApothem,
                        hexRadius);
                }
            }
        }

        private Triplet<VectorXYInt> CreateIndexTriplet(
            PointXY point,
            VectorXYInt mainIndex,
            VectorXY[] normalizedHexVertices,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius)
        {
            VectorXY mainCenter = GetHexCenter(mainIndex, hexOrigin, hexApothem, hexRadius);
            HexVertex nearestVertex = (HexVertex)GetClosestVertexIndex(
                point,
                mainCenter,
                hexRadius,
                normalizedHexVertices,
                out _);
            return mainIndex.GetAdjacentTriplet(nearestVertex, Topology.Layout);
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

        private VectorXY GetHexCenter(
            VectorXYInt index,
            VectorXY hexOrigin,
            float hexApothem,
            float hexRadius)
        {
            switch (Topology.Layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        hexOrigin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? hexApothem : 0f),
                        hexOrigin.Y + 1.5f * hexRadius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        hexOrigin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? -hexApothem : 0f),
                        hexOrigin.Y + 1.5f * hexRadius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        hexOrigin.X + 1.5f * hexRadius * index.X,
                        hexOrigin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? hexApothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        hexOrigin.X + 1.5f * hexRadius * index.X,
                        hexOrigin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? -hexApothem : 0f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(Topology));
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
