namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides the shortest unsigned distance to a two-dimensional point.
    /// </summary>
    public interface IPointDistanceProvider
    {
        /// <summary>
        /// Returns the shortest unsigned distance from this object to the specified point.
        /// </summary>
        /// <param name="point">The finite point to measure to.</param>
        /// <returns>The shortest unsigned distance to the point.</returns>
        float Distance(PointXY point);
    }
}
