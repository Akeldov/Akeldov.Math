using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides rasterization extension methods.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a source object on the specified spatial raster grid using the specified rasterizer.
        /// </summary>
        /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
        /// <typeparam name="TValue">The raster cell value type produced by the rasterizer.</typeparam>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <param name="rasterizer">The rasterization strategy.</param>
        /// <returns>The spatial raster produced from the source object.</returns>
        public static SpatialRaster<TValue> Rasterize<TSource, TValue>(
            this TSource source,
            SpatialRasterGrid grid,
            ISpatialRasterizer<TSource, TValue> rasterizer)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (rasterizer == null)
                throw new ArgumentNullException(nameof(rasterizer));

            return rasterizer.Rasterize(source, grid);
        }

        /// <summary>
        /// Rasterizes the specified grid using the provided raster geometry and value-to-color selector.
        /// </summary>
        /// <typeparam name="TGrid">The grid.</typeparam>
        /// <typeparam name="TValue">The grid value type.</typeparam>
        /// <typeparam name="TColor">The raster color value type.</typeparam>
        /// <param name="grid">The grid to rasterize.</param>
        /// <param name="colorSelector">The function that maps each grid value to a raster color.</param>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public static Raster<TColor> Rasterize<TGrid, TValue, TColor>(
            this TGrid grid,
            Func<TValue, TColor> colorSelector)
            where TGrid : IGrid<TValue>
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (colorSelector == null)
                throw new ArgumentNullException(nameof(colorSelector));

            var resolution = new VectorXYInt(grid.Width, grid.Height);

            int count = checked(grid.Width * grid.Height);
            var values = new TColor[count];

            for (int i = 0; i < values.Length; i++)
                values[i] = colorSelector(grid[i]);

            return new Raster<TColor>(resolution, values);
        }
    }
}
