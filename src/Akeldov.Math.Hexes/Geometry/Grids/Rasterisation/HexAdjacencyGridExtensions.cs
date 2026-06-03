using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    public static class HexAdjacencyGridExtensions
    {
        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexAdjacencyGrid grid,
            Func<IndexedHexAdjacency, RGBA16BitColor> adjacencyToColor)
        {
            return grid.ToRGBA16BitRaster(adjacencyToColor, default(RGBA16BitColor));
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexAdjacencyGrid grid,
            Func<IndexedHexAdjacency, RGBA16BitColor> adjacencyToColor,
            RGBA16BitColor emptyColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (adjacencyToColor == null)
                throw new ArgumentNullException(nameof(adjacencyToColor));

            var values = new RGBA16BitColor[grid.Count];
            bool[] hasHex = grid.HasHex;
            IndexedHexAdjacency[] adjacent = grid.Adjacent;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = hasHex[i]
                    ? adjacencyToColor(adjacent[i])
                    : emptyColor;
            }

            return new RGBA16BitRaster(CreateRasterGrid(grid), values);
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexAdjacencyGrid grid,
            Func<int, HexAdjacency, RGBA16BitColor> hexToColor)
        {
            return grid.ToRGBA16BitRaster(hexToColor, default(RGBA16BitColor));
        }

        public static RGBA16BitRaster ToRGBA16BitRaster(
            this HexAdjacencyGrid grid,
            Func<int, HexAdjacency, RGBA16BitColor> hexToColor,
            RGBA16BitColor emptyColor)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (hexToColor == null)
                throw new ArgumentNullException(nameof(hexToColor));

            var values = new RGBA16BitColor[grid.Count];
            bool[] hasHex = grid.HasHex;
            IndexedHexAdjacency[] adjacent = grid.Adjacent;

            for (int i = 0; i < values.Length; i++)
            {
                IndexedHexAdjacency adjacency = adjacent[i];
                values[i] = hasHex[i]
                    ? hexToColor(adjacency.Index, ToHexAdjacency(adjacency))
                    : emptyColor;
            }

            return new RGBA16BitRaster(CreateRasterGrid(grid), values);
        }

        private static HexAdjacency ToHexAdjacency(IndexedHexAdjacency adjacency)
        {
            return new HexAdjacency(
                (HexAdjacencyFlags)adjacency.Flags & HexAdjacencyFlags.AllAdjacent,
                adjacency.Adjacent0Index,
                adjacency.Adjacent1Index,
                adjacency.Adjacent2Index,
                adjacency.Adjacent3Index,
                adjacency.Adjacent4Index,
                adjacency.Adjacent5Index);
        }

        private static RasterGrid CreateRasterGrid(HexAdjacencyGrid grid)
        {
            return new RasterGrid(
                (PointXY)grid.Origin,
                grid.Size,
                grid.Resolution);
        }
    }
}
