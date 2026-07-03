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
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this BarycentricTripletGrid grid,
            Func<Triplet<float>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(grid[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this BarycentricPartialTripletGrid grid,
            Func<PartialTriplet<float>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(grid[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="chromaticIndexPartialTripletGrid">The ChromaticIndexPartialTripletGrid value.</param>
        /// <param name="barycentricCoordinatesToColor">The BarycentricCoordinatesToColor value.</param>
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this BarycentricPartialTripletGrid grid,
            ChromaticIndexPartialTripletGrid chromaticIndexPartialTripletGrid,
            Func<PartialTriplet<float>, PartialTriplet<byte>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(grid[i], chromaticIndexPartialTripletGrid[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }
    }
}
