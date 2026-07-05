using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides extension methods for sampled hex-grid values.
    /// </summary>
    public static class IGridExtensions
    {
        /// <summary>
        /// Rasterizes the specified grid using the provided raster geometry and value-to-color selector.
        /// </summary>
        /// <typeparam name="TValue">The grid value type.</typeparam>
        /// <typeparam name="TColor">The raster color value type.</typeparam>
        /// <param name="grid">The grid to rasterize.</param>
        /// <param name="rasterGrid">The raster geometry. Its resolution must match the grid dimensions.</param>
        /// <param name="colorSelector">The function that maps each grid value to a raster color.</param>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public static Raster<TColor> Rasterize<TValue, TColor>(
            this IGrid<TValue> grid,
            SpatialRasterGrid rasterGrid,
            Func<TValue, TColor> colorSelector)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            if (rasterGrid.Resolution.X != grid.Width ||
                rasterGrid.Resolution.Y != grid.Height)
                throw new ArgumentException("Raster grid resolution must match grid dimensions.", nameof(rasterGrid));

            int count = checked(grid.Width * grid.Height);
            var values = new TColor[count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(grid[i]);

            return new Raster<TColor>(rasterGrid, values);
        }
    }
}
