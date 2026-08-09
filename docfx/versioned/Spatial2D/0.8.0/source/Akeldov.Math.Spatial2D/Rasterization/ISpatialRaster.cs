namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Represents a raster of values associated with a geometry in two-dimensional space.
    /// </summary>
    /// <typeparam name="TValue">The value type returned by the raster.</typeparam>
    public interface ISpatialRaster<out TValue> : IRaster<TValue>
    {
        /// <summary>
        /// Gets the raster geometry.
        /// </summary>
        RasterGeometry Geometry { get; }
    }
}
