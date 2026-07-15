using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Initializes a new instance of the IndexSeptupletGrid type.
    /// </summary>
    public sealed class IndexSeptupletGrid : ISpatialRaster<Septuplet<VectorXYInt>>
    {
        private Septuplet<VectorXYInt>[] _values = Array.Empty<Septuplet<VectorXYInt>>();

        /// <summary>
        /// Initializes a new instance that covers the whole source hex map.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        public IndexSeptupletGrid(HexMapGeometry hexMapGeometry)
            : this(hexMapGeometry, hexMapGeometry.ToRasterGeometry(1f))
        {
        }

        /// <summary>
        /// Initializes a new instance of the IndexSeptupletGrid type.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public IndexSeptupletGrid(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;
            _values = new Septuplet<VectorXYInt>[checked(Resolution.X * Resolution.Y)];

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
        /// Gets the Resolution value.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

        /// <summary>
        /// Gets the value at the specified grid coordinates.
        /// </summary>
        /// <param name="x">The horizontal grid coordinate.</param>
        /// <param name="y">The vertical grid coordinate.</param>
        public Septuplet<VectorXYInt> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Septuplet<VectorXYInt> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Resolution.X ||
                    index.Y < 0 || index.Y >= Resolution.Y)
                    throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");

                return _values[index.Y * Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index value.</param>
        public Septuplet<VectorXYInt> this[int index]
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
                    _values[index] = new Septuplet<VectorXYInt>(
                        main,
                        main + offsets[0], main + offsets[1], main + offsets[2],
                        main + offsets[3], main + offsets[4], main + offsets[5]);
                }
            }
        }

        private void FillEvenR()
        {
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.RowShiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.RowUnshiftedVectors;
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
                    _values[index] = new Septuplet<VectorXYInt>(
                        main,
                        main + offsets[0], main + offsets[1], main + offsets[2],
                        main + offsets[3], main + offsets[4], main + offsets[5]);
                }
            }
        }

        private void FillOddQ()
        {
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.ColumnUnshiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.ColumnShiftedVectors;
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
                    _values[index] = new Septuplet<VectorXYInt>(
                        main,
                        main + offsets[0], main + offsets[1], main + offsets[2],
                        main + offsets[3], main + offsets[4], main + offsets[5]);
                }
            }
        }

        private void FillEvenQ()
        {
            VectorXYInt[] evenOffsets = HexAdjacencyOffsets.ColumnShiftedVectors;
            VectorXYInt[] oddOffsets = HexAdjacencyOffsets.ColumnUnshiftedVectors;
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
                    _values[index] = new Septuplet<VectorXYInt>(
                        main,
                        main + offsets[0], main + offsets[1], main + offsets[2],
                        main + offsets[3], main + offsets[4], main + offsets[5]);
                }
            }
        }
    }
}
