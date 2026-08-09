namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides signed and unsigned distances to a two-dimensional point.
    /// </summary>
    public interface ISignedPointDistanceProvider : IPointDistanceProvider
    {
        /// <summary>
        /// Returns the signed distance from this object to the specified point.
        /// </summary>
        /// <param name="point">The finite point to measure to.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>
        /// The signed distance to the point. Negative values conventionally represent points inside the object.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has a non-finite coordinate, or when
        /// <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon);
    }
}
