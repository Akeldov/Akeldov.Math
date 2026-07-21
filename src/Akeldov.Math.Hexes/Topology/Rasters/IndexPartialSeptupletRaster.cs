using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Rasterizes the in-bounds members of a central hex and its six neighbors.
    /// </summary>
    public sealed class IndexPartialSeptupletRaster : ISpatialRaster<PartialSeptuplet<VectorXYInt>>
    {
        private PartialSeptuplet<VectorXYInt>[] _values = Array.Empty<PartialSeptuplet<VectorXYInt>>();

        /// <summary>
        /// Initializes a new instance that covers the whole source hex map.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        public IndexPartialSeptupletRaster(HexMapGeometry hexMapGeometry)
            : this(hexMapGeometry, hexMapGeometry.ToRasterGeometry(1f))
        {
        }

        /// <summary>
        /// Initializes a clipped neighborhood-index raster over the specified sampling geometry.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public IndexPartialSeptupletRaster(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;
            _values = new PartialSeptuplet<VectorXYInt>[checked(Resolution.X * Resolution.Y)];

            Fill();
        }

        /// <summary>
        /// Gets the source hex map geometry sampled by the raster.
        /// </summary>
        public HexMapGeometry SourceHexMapGeometry { get; }

        /// <summary>
        /// Gets the geometry used to sample the raster.
        /// </summary>
        public RasterGeometry Geometry { get; }

        /// <summary>
        /// Gets the raster resolution in cells.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

        /// <summary>
        /// Gets the in-bounds neighborhood at the specified raster coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public PartialSeptuplet<VectorXYInt> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the in-bounds neighborhood at the specified raster coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the raster cell.</param>
        public PartialSeptuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Resolution.X ||
                    index.Y < 0 || index.Y >= Resolution.Y)
                    throw new IndexOutOfRangeException($"Raster index out of bounds: {index}");

                return _values[index.Y * Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the in-bounds neighborhood at the specified flat raster index.
        /// </summary>
        /// <param name="index">The zero-based row-major raster index.</param>
        public PartialSeptuplet<VectorXYInt> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private void Fill()
        {
            switch (SourceHexMapGeometry.Topology.Layout)
            {
                case Layout.OddR: FillOddR(); break;
                case Layout.EvenR: FillEvenR(); break;
                case Layout.OddQ: FillOddQ(); break;
                case Layout.EvenQ: FillEvenQ(); break;
                default: throw new ArgumentOutOfRangeException(nameof(SourceHexMapGeometry.Topology.Layout));
            }
        }

        private void FillOddR()
        {
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.RowUnshiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.RowShiftedVectors;
            VectorXYInt hexResolution = SourceHexMapGeometry.Topology.Resolution;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * cellSize.X, pointY);
                    VectorXYInt main = point.ToOddRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    VectorXYInt[] offsets = (main.Y & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt adjacent0 = main + offsets[0];
                    VectorXYInt adjacent1 = main + offsets[1];
                    VectorXYInt adjacent2 = main + offsets[2];
                    VectorXYInt adjacent3 = main + offsets[3];
                    VectorXYInt adjacent4 = main + offsets[4];
                    VectorXYInt adjacent5 = main + offsets[5];
                    bool hasMain = (uint)main.X < (uint)hexResolution.X && (uint)main.Y < (uint)hexResolution.Y;
                    _values[index] = new PartialSeptuplet<VectorXYInt>(
                        main, adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5,
                        hasMain,
                        hasMain && (uint)adjacent0.X < (uint)hexResolution.X && (uint)adjacent0.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent1.X < (uint)hexResolution.X && (uint)adjacent1.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent2.X < (uint)hexResolution.X && (uint)adjacent2.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent3.X < (uint)hexResolution.X && (uint)adjacent3.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent4.X < (uint)hexResolution.X && (uint)adjacent4.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent5.X < (uint)hexResolution.X && (uint)adjacent5.Y < (uint)hexResolution.Y);
                }
            }
        }

        private void FillEvenR()
        {
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.RowShiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.RowUnshiftedVectors;
            VectorXYInt hexResolution = SourceHexMapGeometry.Topology.Resolution;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * cellSize.X, pointY);
                    VectorXYInt main = point.ToEvenRXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    VectorXYInt[] offsets = (main.Y & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt adjacent0 = main + offsets[0];
                    VectorXYInt adjacent1 = main + offsets[1];
                    VectorXYInt adjacent2 = main + offsets[2];
                    VectorXYInt adjacent3 = main + offsets[3];
                    VectorXYInt adjacent4 = main + offsets[4];
                    VectorXYInt adjacent5 = main + offsets[5];
                    bool hasMain = (uint)main.X < (uint)hexResolution.X && (uint)main.Y < (uint)hexResolution.Y;
                    _values[index] = new PartialSeptuplet<VectorXYInt>(
                        main, adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5,
                        hasMain,
                        hasMain && (uint)adjacent0.X < (uint)hexResolution.X && (uint)adjacent0.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent1.X < (uint)hexResolution.X && (uint)adjacent1.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent2.X < (uint)hexResolution.X && (uint)adjacent2.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent3.X < (uint)hexResolution.X && (uint)adjacent3.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent4.X < (uint)hexResolution.X && (uint)adjacent4.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent5.X < (uint)hexResolution.X && (uint)adjacent5.Y < (uint)hexResolution.Y);
                }
            }
        }

        private void FillOddQ()
        {
            VectorXYInt[] evenOffsets = BoolExtensions.ColumnUnshiftedEdgeOffsets;
            VectorXYInt[] oddOffsets = BoolExtensions.ColumnShiftedEdgeOffsets;
            VectorXYInt hexResolution = SourceHexMapGeometry.Topology.Resolution;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * cellSize.X, pointY);
                    VectorXYInt main = point.ToOddQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    VectorXYInt[] offsets = (main.X & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt adjacent0 = main + offsets[0];
                    VectorXYInt adjacent1 = main + offsets[1];
                    VectorXYInt adjacent2 = main + offsets[2];
                    VectorXYInt adjacent3 = main + offsets[3];
                    VectorXYInt adjacent4 = main + offsets[4];
                    VectorXYInt adjacent5 = main + offsets[5];
                    bool hasMain = (uint)main.X < (uint)hexResolution.X && (uint)main.Y < (uint)hexResolution.Y;
                    _values[index] = new PartialSeptuplet<VectorXYInt>(
                        main, adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5,
                        hasMain,
                        hasMain && (uint)adjacent0.X < (uint)hexResolution.X && (uint)adjacent0.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent1.X < (uint)hexResolution.X && (uint)adjacent1.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent2.X < (uint)hexResolution.X && (uint)adjacent2.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent3.X < (uint)hexResolution.X && (uint)adjacent3.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent4.X < (uint)hexResolution.X && (uint)adjacent4.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent5.X < (uint)hexResolution.X && (uint)adjacent5.Y < (uint)hexResolution.Y);
                }
            }
        }

        private void FillEvenQ()
        {
            VectorXYInt[] evenOffsets = BoolExtensions.ColumnShiftedEdgeOffsets;
            VectorXYInt[] oddOffsets = BoolExtensions.ColumnUnshiftedEdgeOffsets;
            VectorXYInt hexResolution = SourceHexMapGeometry.Topology.Resolution;
            int width = Resolution.X;
            VectorXY cellSize = Geometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                float pointY = Geometry.Origin.Y + (y + 0.5f) * cellSize.Y;
                for (int x = 0; x < width; x++, index++)
                {
                    var point = new PointXY(Geometry.Origin.X + (x + 0.5f) * cellSize.X, pointY);
                    VectorXYInt main = point.ToEvenQXYIndex(SourceHexMapGeometry.Radius, SourceHexMapGeometry.Origin);
                    VectorXYInt[] offsets = (main.X & 1) == 0 ? evenOffsets : oddOffsets;
                    VectorXYInt adjacent0 = main + offsets[0];
                    VectorXYInt adjacent1 = main + offsets[1];
                    VectorXYInt adjacent2 = main + offsets[2];
                    VectorXYInt adjacent3 = main + offsets[3];
                    VectorXYInt adjacent4 = main + offsets[4];
                    VectorXYInt adjacent5 = main + offsets[5];
                    bool hasMain = (uint)main.X < (uint)hexResolution.X && (uint)main.Y < (uint)hexResolution.Y;
                    _values[index] = new PartialSeptuplet<VectorXYInt>(
                        main, adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5,
                        hasMain,
                        hasMain && (uint)adjacent0.X < (uint)hexResolution.X && (uint)adjacent0.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent1.X < (uint)hexResolution.X && (uint)adjacent1.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent2.X < (uint)hexResolution.X && (uint)adjacent2.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent3.X < (uint)hexResolution.X && (uint)adjacent3.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent4.X < (uint)hexResolution.X && (uint)adjacent4.Y < (uint)hexResolution.Y,
                        hasMain && (uint)adjacent5.X < (uint)hexResolution.X && (uint)adjacent5.Y < (uint)hexResolution.Y);
                }
            }
        }
    }
}
