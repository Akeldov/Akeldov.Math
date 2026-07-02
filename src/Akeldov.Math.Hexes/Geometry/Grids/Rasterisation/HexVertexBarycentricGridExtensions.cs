using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class HexVertexBarycentricGridExtensions
    {
        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this BarycentricTripletGrid grid,
            Func<Triplet<float>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];
            Triplet<float>[] barycentricCoordinates = grid.BarycentricCoordinateStorage;

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(barycentricCoordinates[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }

        public static Raster<RGBA16BitColor> ToRGBA16BitRaster(
            this BarycentricPartialTripletGrid grid,
            Func<PartialTriplet<float>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];
            PartialTriplet<float>[] barycentricCoordinates = grid.BarycentricCoordinateStorage;

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(barycentricCoordinates[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }

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
            PartialTriplet<float>[] barycentricCoordinates = grid.BarycentricCoordinateStorage;
            PartialTriplet<byte>[] chromaticIndices = chromaticIndexPartialTripletGrid.ChromaticIndexStorage;

            for (int i = 0; i < values.Length; i++)
                values[i] = barycentricCoordinatesToColor(barycentricCoordinates[i], chromaticIndices[i]);

            var rasterGrid = new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);

            return new Raster<RGBA16BitColor>(rasterGrid, values);
        }
    }
}
