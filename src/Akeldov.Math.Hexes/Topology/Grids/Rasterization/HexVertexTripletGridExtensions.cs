using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class HexVertexTripletGridExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="tripletToColor">The TripletToColor value.</param>
        public static SpatialRaster<RGBA16BitColor> ToRGBA16BitRaster(
            this IndexTripletGrid grid,
            Func<Triplet<VectorXYInt>, RGBA16BitColor> tripletToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), tripletToColor);
        }

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="grid">The Grid value.</param>
        /// <param name="tripletToColor">The TripletToColor value.</param>
        public static SpatialRaster<RGBA16BitColor> ToRGBA16BitRaster(
            this IndexPartialTripletGrid grid,
            Func<PartialTriplet<VectorXYInt>, RGBA16BitColor> tripletToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            return grid.Rasterize(CreateRasterGrid(grid), tripletToColor);
        }

        private static SpatialRasterGrid CreateRasterGrid(IndexTripletGrid grid)
        {
            return new SpatialRasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static SpatialRasterGrid CreateRasterGrid(IndexPartialTripletGrid grid)
        {
            return new SpatialRasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
