namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Defines a rasterization strategy that samples a source object into a raster without spatial bounds.
    /// </summary>
    /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
    /// <typeparam name="TValue">The raster cell value type produced by the rasterizer.</typeparam>
    public interface IRasterizer<in TSource, TValue>
    {
        /// <summary>
        /// Rasterizes the specified source object at the specified raster resolution.
        /// </summary>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="resolution">The raster resolution in cells.</param>
        /// <returns>The raster produced from the source object.</returns>
        Raster<TValue> Rasterize(TSource source, VectorXYInt resolution);
    }
}
