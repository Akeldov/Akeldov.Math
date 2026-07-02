using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class HexVertexTripletGridExtensions
    {
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this ChromaticIndexTripletGrid grid,
            Func<Triplet<byte>, RGBA16BitColor> chromaticIndicesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (chromaticIndicesToColor == null)
                throw new ArgumentNullException(nameof(chromaticIndicesToColor));

            var values = new RGBA16BitColor[grid.Count];
            Triplet<byte>[] chromaticIndices = grid.ChromaticIndices;

            for (int i = 0; i < values.Length; i++)
                values[i] = chromaticIndicesToColor(chromaticIndices[i]);

            return new Raster<RGBA16BitColor>(CreateRasterGrid(grid), values);
        }

        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this ChromaticIndexPartialTripletGrid grid,
            Func<PartialTriplet<byte>, RGBA16BitColor> chromaticIndicesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (chromaticIndicesToColor == null)
                throw new ArgumentNullException(nameof(chromaticIndicesToColor));

            var values = new RGBA16BitColor[grid.Count];
            PartialTriplet<byte>[] chromaticIndices = grid.ChromaticIndices;

            for (int i = 0; i < values.Length; i++)
                values[i] = chromaticIndicesToColor(chromaticIndices[i]);

            return new Raster<RGBA16BitColor>(CreateRasterGrid(grid), values);
        }

        private static RasterGrid CreateRasterGrid(ChromaticIndexTripletGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static RasterGrid CreateRasterGrid(ChromaticIndexPartialTripletGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
