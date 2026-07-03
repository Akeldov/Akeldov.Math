using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology.Grids.Rasterization
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class IndexedHexAdjacencyGridExtensions
    {
        /// <summary>
        /// Rasterizes the specified hex-grid data.
        /// </summary>
        /// <param name="indexedHexAdjacencyGrid">The IndexedHexAdjacencyGrid value.</param>
        /// <param name="colorSelector">The ColorSelector value.</param>
        public static Raster<RGBA16BitColor> Rasterize(
            this IndexSeptupletGrid indexedHexAdjacencyGrid,
            Func<Septuplet<VectorXYInt>, RGBA16BitColor> colorSelector)
        {
            if (indexedHexAdjacencyGrid == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyGrid));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            var values = new RGBA16BitColor[indexedHexAdjacencyGrid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(indexedHexAdjacencyGrid[i]);

            return new Raster<RGBA16BitColor>(
                new RasterGrid(
                    new PointXY(0f, 0f),
                    new VectorXY(indexedHexAdjacencyGrid.Width, indexedHexAdjacencyGrid.Height),
                    indexedHexAdjacencyGrid.Resolution),
                values);
        }

        /// <summary>
        /// Rasterizes the specified hex-grid data.
        /// </summary>
        /// <param name="indexedHexAdjacencyGrid">The IndexedHexAdjacencyGrid value.</param>
        /// <param name="colorSelector">The ColorSelector value.</param>
        public static Raster<RGBA16BitColor> Rasterize(
            this IndexPartialSeptupletGrid indexedHexAdjacencyGrid,
            Func<PartialSeptuplet<VectorXYInt>, RGBA16BitColor> colorSelector)
        {
            if (indexedHexAdjacencyGrid == null)
                throw new ArgumentNullException(nameof(indexedHexAdjacencyGrid));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            var values = new RGBA16BitColor[indexedHexAdjacencyGrid.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(indexedHexAdjacencyGrid[i]);

            return new Raster<RGBA16BitColor>(
                new RasterGrid(
                    new PointXY(0f, 0f),
                    new VectorXY(indexedHexAdjacencyGrid.Width, indexedHexAdjacencyGrid.Height),
                    indexedHexAdjacencyGrid.Resolution),
                values);
        }
    }
}
