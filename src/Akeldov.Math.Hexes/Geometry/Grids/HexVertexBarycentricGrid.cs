using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public sealed class HexVertexBarycentricGrid
    {
        private readonly HexGridDefinition _grid;
        private readonly Triplet<float>[] _barycentricCoordinates;
        private readonly bool[] _hasHex;

        public HexVertexBarycentricGrid(
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

        public HexVertexBarycentricGrid(
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

        private HexVertexBarycentricGrid(HexGridDefinition grid, HexVertexTripletGridFillMode fillMode)
        {
            _grid = grid;
            FillMode = fillMode;
            _barycentricCoordinates = new Triplet<float>[grid.Count];
            _hasHex = new bool[grid.Count];

            HexVertexTripletGridBuilder.Fill(grid, null, _barycentricCoordinates, null, _hasHex, fillMode);
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
        public int Count => _barycentricCoordinates.Length;
        public Triplet<float>[] BarycentricCoordinates => _barycentricCoordinates;
        public bool[] HasHex => _hasHex;
        public HexVertexTripletGridFillMode FillMode { get; }

        public Triplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _barycentricCoordinates[GetHitFlatIndex(index)];
        }

        public Triplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfNoHex(index);
                return _barycentricCoordinates[index];
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

        public bool TryGetBarycentricCoordinates(VectorXYInt gridIndex, out Triplet<float> barycentricCoordinates)
        {
            ThrowIfGridIndexOutOfBounds(gridIndex);

            int flatIndex = _grid.GetFlatIndex(gridIndex);
            barycentricCoordinates = _barycentricCoordinates[flatIndex];
            return _hasHex[flatIndex];
        }

        public Triplet<float> GetBarycentricCoordinates(VectorXYInt gridIndex)
        {
            return _barycentricCoordinates[GetHitFlatIndex(gridIndex)];
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
                throw new InvalidOperationException("Grid cell does not hit a hex in the vertex barycentric grid.");
        }

        private void ThrowIfGridIndexOutOfBounds(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= ResolutionX ||
                index.Y < 0 || index.Y >= ResolutionY)
                throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");
        }
    }
}
