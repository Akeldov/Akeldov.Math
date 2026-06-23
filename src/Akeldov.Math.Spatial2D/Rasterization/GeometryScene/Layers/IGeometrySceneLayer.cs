namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Defines a geometry scene layer that can be sampled into a color value and composited.
    /// </summary>
    /// <typeparam name="TColor">The color value type produced by this layer.</typeparam>
    public interface IGeometrySceneLayer<TColor>
    {
        /// <summary>
        /// Samples this layer at the specified finite point.
        /// </summary>
        /// <param name="point">The finite point to sample.</param>
        /// <returns>The color contributed by this layer at <paramref name="point"/>.</returns>
        TColor Sample(PointXY point);
    }
}
