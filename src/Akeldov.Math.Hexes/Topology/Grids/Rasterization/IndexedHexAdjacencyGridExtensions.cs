using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology.Grids.Rasterization
{
    public static class IndexedHexAdjacencyGridExtensions
    {
        public static RGBA16BitRaster Rasterize(
            this IndexedHexAdjacencyGrid indexedHexAdjacencyGrid,
            Func<Septuplet<int>, RGBA16BitColor> colorSelector)
        {
            if (indexedHexAdjacencyGrid == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyGrid));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            var values = new RGBA16BitColor[indexedHexAdjacencyGrid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(indexedHexAdjacencyGrid[i]);

            return new RGBA16BitRaster(
                new RasterGrid(
                    new PointXY(0f, 0f),
                    new VectorXY(indexedHexAdjacencyGrid.Width, indexedHexAdjacencyGrid.Height),
                    indexedHexAdjacencyGrid.Resolution),
                values);
        }
    }
}
