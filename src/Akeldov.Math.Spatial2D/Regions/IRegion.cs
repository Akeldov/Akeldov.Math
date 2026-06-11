namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a filled two-dimensional region.
    /// </summary>
    public interface IRegion
    {
        /// <summary>
        /// Determines whether the specified point lies inside or on this region.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns><see langword="true"/> if the point lies inside or on the region; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        bool Contains(
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon);
    }
}
