namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Defines a rasterization strategy that converts a source object into a raster artifact.
    /// </summary>
    /// <typeparam name="TSource">The source object type to rasterize.</typeparam>
    /// <typeparam name="TRaster">The raster artifact type produced by the rasterizer.</typeparam>
    public interface IRasterizer<in TSource, out TRaster>
    {
        /// <summary>
        /// Rasterizes the specified source object on the specified raster grid.
        /// </summary>
        /// <param name="source">The source object to rasterize.</param>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>The raster artifact produced from the source object.</returns>
        TRaster Rasterize(TSource source, SpatialRasterGrid grid);
    }
}
