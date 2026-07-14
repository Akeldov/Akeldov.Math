namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Defines a spatial rasterization strategy that samples a source object into a spatial raster.
    /// </summary>
    /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
    /// <typeparam name="TValue">The raster cell value type produced by the rasterizer.</typeparam>
    public interface ISpatialRasterizer<in TSource, TValue>
    {
        /// <summary>
        /// Rasterizes the specified source object on the specified spatial raster grid.
        /// </summary>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>The spatial raster produced from the source object.</returns>
        SpatialRaster<TValue> Rasterize(TSource source, SpatialRasterGrid grid);
    }
}
