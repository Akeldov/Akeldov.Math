using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents partial barycentric coordinates sampled from a bounded hex map and ordered by chromatic index.
    /// </summary>
    public sealed class ChromaticBarycentricPartialTripletGrid : ISpatialRaster<PartialChromaticTriplet<float>>
    {
        private readonly PartialChromaticTriplet<float>[] _values;

        /// <summary>
        /// Initializes a new instance that covers the whole source hex map.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        public ChromaticBarycentricPartialTripletGrid(HexMapGeometry hexMapGeometry)
            : this(hexMapGeometry, hexMapGeometry.ToRasterGeometry(1f))
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified sampling geometry.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public ChromaticBarycentricPartialTripletGrid(
            HexMapGeometry hexMapGeometry,
            RasterGeometry rasterGeometry)
        {
            if (hexMapGeometry.Topology.Resolution.X <= 0 || hexMapGeometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(hexMapGeometry), hexMapGeometry, "Hex map dimensions must be positive.");

            if (rasterGeometry.Resolution.X <= 0 || rasterGeometry.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rasterGeometry), rasterGeometry, "Raster geometry resolution components must be positive.");

            SourceHexMapGeometry = hexMapGeometry;
            Geometry = rasterGeometry;
            _values = new PartialChromaticTriplet<float>[checked(Resolution.X * Resolution.Y)];

            var barycentricGrid = new BarycentricPartialTripletGrid(hexMapGeometry, rasterGeometry);
            var chromaticIndexGrid = new ChromaticIndexTripletGrid(hexMapGeometry, rasterGeometry);

            for (int index = 0; index < _values.Length; index++)
            {
                PartialTriplet<float> barycentric = barycentricGrid[index];
                Triplet<byte> chromaticIndices = chromaticIndexGrid[index];
                (float Coordinate, bool IsPresent) index0 = GetCoordinate(0, barycentric, chromaticIndices);
                (float Coordinate, bool IsPresent) index1 = GetCoordinate(1, barycentric, chromaticIndices);
                (float Coordinate, bool IsPresent) index2 = GetCoordinate(2, barycentric, chromaticIndices);
                _values[index] = new PartialChromaticTriplet<float>(
                    index0.Coordinate,
                    index1.Coordinate,
                    index2.Coordinate,
                    index0.IsPresent,
                    index1.IsPresent,
                    index2.IsPresent);
            }
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
        /// Gets the raster resolution.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

        /// <summary>
        /// Gets the partial chromatically ordered barycentric coordinates at the specified raster coordinates.
        /// </summary>
        /// <param name="x">The zero-based raster column.</param>
        /// <param name="y">The zero-based raster row.</param>
        public PartialChromaticTriplet<float> this[int x, int y] => _values[y * Resolution.X + x];

        /// <summary>
        /// Gets the partial chromatically ordered barycentric coordinates at the specified raster index.
        /// </summary>
        /// <param name="index">The zero-based two-dimensional raster index.</param>
        public PartialChromaticTriplet<float> this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index.X >= (uint)Resolution.X ||
                    (uint)index.Y >= (uint)Resolution.Y)
                    throw new IndexOutOfRangeException($"Grid index out of bounds: {index}");

                return _values[index.Y * Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the partial chromatically ordered barycentric coordinates at the specified flat row-major index.
        /// </summary>
        /// <param name="index">The zero-based flat row-major raster index.</param>
        public PartialChromaticTriplet<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        /// <summary>
        /// Tries to get partial chromatically ordered barycentric coordinates at the specified raster index.
        /// </summary>
        /// <param name="gridIndex">The zero-based two-dimensional raster index.</param>
        /// <param name="barycentricCoordinates">The coordinates when the index is inside the raster; otherwise, the default value.</param>
        /// <returns><see langword="true"/> when the index is inside the raster; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(
            VectorXYInt gridIndex,
            out PartialChromaticTriplet<float> barycentricCoordinates)
        {
            if ((uint)gridIndex.X >= (uint)Resolution.X ||
                (uint)gridIndex.Y >= (uint)Resolution.Y)
            {
                barycentricCoordinates = default;
                return false;
            }

            barycentricCoordinates = _values[gridIndex.Y * Resolution.X + gridIndex.X];
            return true;
        }

        private static (float Coordinate, bool IsPresent) GetCoordinate(
            byte chromaticIndex,
            PartialTriplet<float> barycentric,
            Triplet<byte> chromaticIndices)
        {
            if (chromaticIndices.Main == chromaticIndex)
                return (barycentric.Main, barycentric.HasMain);

            if (chromaticIndices.Left == chromaticIndex)
                return (barycentric.Left, barycentric.HasLeft);

            if (chromaticIndices.Right == chromaticIndex)
                return (barycentric.Right, barycentric.HasRight);

            throw new InvalidOperationException($"Chromatic index triplet does not contain index {chromaticIndex}.");
        }
    }
}
