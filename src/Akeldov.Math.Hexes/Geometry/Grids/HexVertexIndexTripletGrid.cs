using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexVertexIndexTripletGrid
    {
        private static readonly Triplet<VectorXYInt> InvalidIndexTriplet = new Triplet<VectorXYInt>(
            new VectorXYInt(-1, -1),
            new VectorXYInt(-1, -1),
            new VectorXYInt(-1, -1));

        private readonly HexGridDefinition _grid;
        private readonly Triplet<VectorXYInt>[] _indexTriplets;
        private readonly bool[] _hasHex;

        public HexVertexIndexTripletGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXYInt resolution,
            HexVertexTripletGridFillMode fillMode = HexVertexTripletGridFillMode.HitHexesOnly)
            : this(HexGridDefinition.Create(hexWidth, hexHeight, layout, hexOrigin, hexApothem, resolution), fillMode)
        {
        }

        public HexVertexIndexTripletGrid(
            int hexWidth,
            int hexHeight,
            Layout layout,
            VectorXY hexOrigin,
            float hexApothem,
            VectorXY gridOrigin,
            VectorXY gridSize,
            VectorXYInt resolution,
            HexVertexTripletGridFillMode fillMode = HexVertexTripletGridFillMode.HitHexesOnly)
            : this(HexGridDefinition.Create(hexWidth, hexHeight, layout, hexOrigin, hexApothem, gridOrigin, gridSize, resolution), fillMode)
        {
        }

        private HexVertexIndexTripletGrid(HexGridDefinition grid, HexVertexTripletGridFillMode fillMode)
        {
            _grid = grid;
            FillMode = fillMode;
            _indexTriplets = new Triplet<VectorXYInt>[grid.Count];
            _hasHex = new bool[grid.Count];

            HexVertexTripletGridBuilder.Fill(grid, _indexTriplets, null, null, _hasHex, fillMode);
        }

        public VectorXYInt HexResolution => _grid.HexResolution;
        public Layout Layout => _grid.Layout;
        public VectorXY HexOrigin => _grid.HexOrigin;
        public float HexApothem => _grid.HexApothem;
        public float HexRadius => _grid.HexRadius;
        public VectorXY Origin => _grid.Origin;
        public VectorXY Size => _grid.Size;
        public VectorXY CellSize => _grid.CellSize;
        public VectorXYInt Resolution => _grid.Resolution;
        public int ResolutionX => _grid.ResolutionX;
        public int ResolutionY => _grid.ResolutionY;
        public int Count => _indexTriplets.Length;
        public Triplet<VectorXYInt>[] IndexTriplets => _indexTriplets;
        public bool[] HasHex => _hasHex;
        public HexVertexTripletGridFillMode FillMode { get; }

        public Triplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indexTriplets[GetHitFlatIndex(index)];
        }

        public Triplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfNoHex(index);
                return _indexTriplets[index];
            }
        }

        public VectorXY GetCellCenter(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return _grid.GetCellCenterUnchecked(index.X, index.Y);
        }

        public bool HasHexAt(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);
            return _hasHex[_grid.GetFlatIndex(index)];
        }

        public bool TryGetIndexTriplet(VectorXYInt gridIndex, out Triplet<VectorXYInt> indexTriplet)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = _grid.GetFlatIndex(gridIndex);
            indexTriplet = _hasHex[flatIndex] ? _indexTriplets[flatIndex] : InvalidIndexTriplet;
            return _hasHex[flatIndex];
        }

        public Triplet<VectorXYInt> GetIndexTriplet(VectorXYInt gridIndex)
        {
            return _indexTriplets[GetHitFlatIndex(gridIndex)];
        }

        private int GetHitFlatIndex(VectorXYInt index)
        {
            ThrowIfGridIndexOutOfBounds(index);

            int flatIndex = _grid.GetFlatIndex(index);
            ThrowIfNoHex(flatIndex);
            return flatIndex;
        }

        private void ThrowIfNoHex(int flatIndex)
        {
            if (!_hasHex[flatIndex])
                throw new InvalidOperationException("Grid cell does not hit a hex in the vertex triplet grid.");
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= ResolutionX ||
                index.Y < 0 || index.Y >= ResolutionY)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }
    }
}
