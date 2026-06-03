using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static class HexVertexTripletGridExtensions
    {
        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexIndexTripletGrid grid,
            Func<Triplet<VectorXYInt>, RGBA16BitColor> tripletToColor)
        {
            return grid.ToRGBA16BitRaster(tripletToColor, default(RGBA16BitColor));
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexIndexTripletGrid grid,
            Func<Triplet<VectorXYInt>, RGBA16BitColor> tripletToColor,
            RGBA16BitColor emptyColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (tripletToColor == null)
                throw new ArgumentNullException(nameof(tripletToColor));

            var values = new RGBA16BitColor[grid.Count];
            bool[] hasHex = grid.HasHex;
            Triplet<VectorXYInt>[] indexTriplets = grid.IndexTriplets;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = hasHex[i]
                    ? tripletToColor(indexTriplets[i])
                    : emptyColor;
            }

            return new RGBA16BitRaster(CreateRasterGrid(grid), values);
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexBarycentricGrid grid,
            Func<Triplet<float>, RGBA16BitColor> barycentricCoordinatesToColor)
        {
            return grid.ToRGBA16BitRaster(barycentricCoordinatesToColor, default(RGBA16BitColor));
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexBarycentricGrid grid,
            Func<Triplet<float>, RGBA16BitColor> barycentricCoordinatesToColor,
            RGBA16BitColor emptyColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (barycentricCoordinatesToColor == null)
                throw new ArgumentNullException(nameof(barycentricCoordinatesToColor));

            var values = new RGBA16BitColor[grid.Count];
            bool[] hasHex = grid.HasHex;
            Triplet<float>[] barycentricCoordinates = grid.BarycentricCoordinates;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = hasHex[i]
                    ? barycentricCoordinatesToColor(barycentricCoordinates[i])
                    : emptyColor;
            }

            return new RGBA16BitRaster(CreateRasterGrid(grid), values);
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexChromaticIndexTripletGrid grid,
            Func<Triplet<byte>, RGBA16BitColor> chromaticIndicesToColor)
        {
            return grid.ToRGBA16BitRaster(chromaticIndicesToColor, default(RGBA16BitColor));
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexVertexChromaticIndexTripletGrid grid,
            Func<Triplet<byte>, RGBA16BitColor> chromaticIndicesToColor,
            RGBA16BitColor emptyColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (chromaticIndicesToColor == null)
                throw new ArgumentNullException(nameof(chromaticIndicesToColor));

            var values = new RGBA16BitColor[grid.Count];
            bool[] hasHex = grid.HasHex;
            Triplet<byte>[] chromaticIndices = grid.ChromaticIndices;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = hasHex[i]
                    ? chromaticIndicesToColor(chromaticIndices[i])
                    : emptyColor;
            }

            return new RGBA16BitRaster(CreateRasterGrid(grid), values);
        }

        private static RasterGrid CreateRasterGrid(HexVertexIndexTripletGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static RasterGrid CreateRasterGrid(HexVertexBarycentricGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }

        private static RasterGrid CreateRasterGrid(HexVertexChromaticIndexTripletGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
