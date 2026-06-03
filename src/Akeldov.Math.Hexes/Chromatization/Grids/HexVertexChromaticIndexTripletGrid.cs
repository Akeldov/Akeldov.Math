using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexVertexChromaticIndexTripletGrid
    {
        private readonly HexGridDefinition _grid;
        private readonly Triplet<byte>[] _chromaticIndices;
        private readonly bool[] _hasHex;

        public HexVertexChromaticIndexTripletGrid(
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

        public HexVertexChromaticIndexTripletGrid(
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

        private HexVertexChromaticIndexTripletGrid(HexGridDefinition grid, HexVertexTripletGridFillMode fillMode)
        {
            _grid = grid;
            FillMode = fillMode;
            _chromaticIndices = new Triplet<byte>[grid.Count];
            _hasHex = new bool[grid.Count];

            HexVertexTripletGridBuilder.Fill(grid, null, null, _chromaticIndices, _hasHex, fillMode);
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
        public int Count => _chromaticIndices.Length;
        public Triplet<byte>[] ChromaticIndices => _chromaticIndices;
        public bool[] HasHex => _hasHex;
        public HexVertexTripletGridFillMode FillMode { get; }

        public Triplet<byte> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _chromaticIndices[GetHitFlatIndex(index)];
        }

        public Triplet<byte> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfNoHex(index);
                return _chromaticIndices[index];
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

        public bool TryGetChromaticIndices(VectorXYInt gridIndex, out Triplet<byte> chromaticIndices)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = _grid.GetFlatIndex(gridIndex);
            chromaticIndices = _chromaticIndices[flatIndex];
            return _hasHex[flatIndex];
        }

        public Triplet<byte> GetChromaticIndices(VectorXYInt gridIndex)
        {
            return _chromaticIndices[GetHitFlatIndex(gridIndex)];
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
                throw new InvalidOperationException("Grid cell does not hit a hex in the vertex chromatic index triplet grid.");
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= ResolutionX ||
                index.Y < 0 || index.Y >= ResolutionY)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }
    }
}
