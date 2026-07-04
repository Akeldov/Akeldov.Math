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

            return indexedHexAdjacencyGrid.Rasterize(
                CreateRasterGrid(indexedHexAdjacencyGrid),
                colorSelector);
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

            return indexedHexAdjacencyGrid.Rasterize(
                CreateRasterGrid(indexedHexAdjacencyGrid),
                colorSelector);
        }

        private static RasterGrid CreateRasterGrid(IndexSeptupletGrid indexedHexAdjacencyGrid)
        {
            return new RasterGrid(
                new PointXY(0f, 0f),
                new VectorXY(indexedHexAdjacencyGrid.Width, indexedHexAdjacencyGrid.Height),
                indexedHexAdjacencyGrid.Resolution);
        }

        private static RasterGrid CreateRasterGrid(IndexPartialSeptupletGrid indexedHexAdjacencyGrid)
        {
            return new RasterGrid(
                new PointXY(0f, 0f),
                new VectorXY(indexedHexAdjacencyGrid.Width, indexedHexAdjacencyGrid.Height),
                indexedHexAdjacencyGrid.Resolution);
        }
    }
}
