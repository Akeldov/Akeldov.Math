using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class HexVertexTripletGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndicesToColor">The ChromaticIndicesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this ChromaticIndexTripletGrid grid,
            Func<Triplet<byte>, TValue> chromaticIndicesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), chromaticIndicesToColor);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndicesToColor">The ChromaticIndicesToColor value.</param>
        public static SpatialRaster<TValue> Rasterize<TValue>(
            this ChromaticIndexPartialTripletGrid grid,
            Func<PartialTriplet<byte>, TValue> chromaticIndicesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), chromaticIndicesToColor);
        }

        private static RasterGeometry CreateRasterGrid(ChromaticIndexTripletGrid grid)
        {
            return new RasterGeometry(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static RasterGeometry CreateRasterGrid(ChromaticIndexPartialTripletGrid grid)
        {
            return new RasterGeometry(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
