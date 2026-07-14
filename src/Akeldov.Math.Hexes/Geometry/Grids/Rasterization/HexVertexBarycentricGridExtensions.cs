using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class HexVertexBarycentricGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricTripletGrid grid,
            Func<Triplet<float>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), barycentricCoordinatesToColor);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricPartialTripletGrid grid,
            Func<PartialTriplet<float>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), barycentricCoordinatesToColor);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndexPartialTripletGrid">The ChromaticIndexPartialTripletGrid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this BarycentricPartialTripletGrid grid,
            ChromaticIndexPartialTripletGrid chromaticIndexPartialTripletGrid,
            Func<PartialTriplet<float>, PartialTriplet<byte>, TValue> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new TValue[grid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(grid[i], chromaticIndexPartialTripletGrid[i]);

            var rasterGrid = new RasterGeometry(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new SpatialRaster<TValue>(rasterGrid, values);
        }

        private static RasterGeometry CreateRasterGrid(BarycentricTripletGrid grid)
        {
            return new RasterGeometry(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static RasterGeometry CreateRasterGrid(BarycentricPartialTripletGrid grid)
        {
            return new RasterGeometry(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
