using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class HexVertexTripletGridExtensions
    {
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this IndexTripletGrid grid,
            Func<Triplet<VectorXYInt>, RGBA16BitColor> tripletToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (tripletToColor == null)
                throw new ArgumentNullException(nameof(tripletToColor));

            var values = new RGBA16BitColor[grid.Count];
            Triplet<VectorXYInt>[] indexTriplets = grid.IndexTriplets;

            for (int i = 0; i < values.Length; i++)
                values[i] = tripletToColor(indexTriplets[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }

        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this IndexPartialTripletGrid grid,
            Func<PartialTriplet<VectorXYInt>, RGBA16BitColor> tripletToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (tripletToColor == null)
                throw new ArgumentNullException(nameof(tripletToColor));

            var values = new RGBA16BitColor[grid.Count];
            PartialTriplet<VectorXYInt>[] indexTriplets = grid.IndexTriplets;

            for (int i = 0; i < values.Length; i++)
                values[i] = tripletToColor(indexTriplets[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }
    }
}
