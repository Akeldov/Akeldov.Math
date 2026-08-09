namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a filled two-dimensional region.
    /// </summary>
    public interface IRegion : ISignedPointDistanceProvider
    {
        /// <summary>
        /// Determines whether the specified point lies inside or on this region.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns><see langword="true"/> if the point lies inside or on the region; otherwise, <see langword="false"/>.</returns>
        bool Contains(PointXY point);
    }
}
