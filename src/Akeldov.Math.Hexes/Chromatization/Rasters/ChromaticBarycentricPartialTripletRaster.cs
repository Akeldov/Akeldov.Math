using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents partial barycentric coordinates sampled from a bounded hex map and ordered by chromatic index.
    /// </summary>
    /// <remarks>
    /// A sample inside the hex map bounding box whose surrounding hexes all lie outside the map, such as a sample
    /// in a corner notch of an offset layout, gets full weight for the chromatic index of the nearest map hex.
    /// A sample outside the bounding box without surrounding map hexes has no present values.
    /// </remarks>
    public sealed class ChromaticBarycentricPartialTripletRaster : ISpatialRaster<PartialChromaticTriplet<float>>
    {
        private readonly PartialChromaticTriplet<float>[] _values;

        /// <summary>
        /// Initializes a new instance that covers the whole source hex map.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        public ChromaticBarycentricPartialTripletRaster(HexMapGeometry hexMapGeometry)
            : this(hexMapGeometry, hexMapGeometry.ToRasterGeometry(1f))
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified sampling geometry.
        /// </summary>
        /// <param name="hexMapGeometry">The source hex map geometry.</param>
        /// <param name="rasterGeometry">The geometry that defines the sampled raster origin, size, and resolution.</param>
        public ChromaticBarycentricPartialTripletRaster(
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

            var barycentricRaster = new BarycentricPartialTripletRaster(hexMapGeometry, rasterGeometry);
            var chromaticIndexRaster = new ChromaticIndexTripletRaster(hexMapGeometry, rasterGeometry);
            Rectangle hexMapBounds = hexMapGeometry.GetBoundingBox();
            VectorXY cellSize = rasterGeometry.CellSize;

            for (int index = 0, y = 0; y < Resolution.Y; y++)
            {
                for (int x = 0; x < Resolution.X; x++, index++)
                {
                    PartialTriplet<float> barycentric = barycentricRaster[index];
                    if (barycentric.Presence == TripletPresence.None)
                    {
                        var point = new PointXY(
                            rasterGeometry.Origin.X + (x + 0.5f) * cellSize.X,
                            rasterGeometry.Origin.Y + (y + 0.5f) * cellSize.Y);
                        _values[index] = hexMapBounds.Contains(point)
                            ? GetNearestHexValue(point, hexMapGeometry)
                            : default;
                        continue;
                    }

                    Triplet<byte> chromaticIndices = chromaticIndexRaster[index];
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
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        index,
                        $"Raster index must satisfy 0 <= X < {Resolution.X} and 0 <= Y < {Resolution.Y}.");

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

        private static PartialChromaticTriplet<float> GetNearestHexValue(PointXY point, HexMapGeometry hexMapGeometry)
        {
            // Inside the bounding box, such samples only occur in corner notches next to a corner map hex,
            // so clamping the containing hex index to the map yields the nearest map hex.
            Layout layout = hexMapGeometry.Topology.Layout;
            VectorXYInt maxHexIndex = new VectorXYInt(
                hexMapGeometry.Topology.Resolution.X - 1,
                hexMapGeometry.Topology.Resolution.Y - 1);
            VectorXYInt hexIndex = point
                .ToXYIndex(hexMapGeometry.Radius, hexMapGeometry.Origin, layout)
                .Clamp(VectorXYInt.Zero, maxHexIndex);
            int chromaticClass = hexIndex.GetChromaticClass(layout);

            return new PartialChromaticTriplet<float>(
                chromaticClass == 0 ? 1f : 0f,
                chromaticClass == 1 ? 1f : 0f,
                chromaticClass == 2 ? 1f : 0f,
                chromaticClass == 0,
                chromaticClass == 1,
                chromaticClass == 2);
        }
    }
}
