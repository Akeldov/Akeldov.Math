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
        /// <returns>
        /// The signed distance to the point. Negative values conventionally represent points inside the object.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has a non-finite coordinate.
        /// </exception>
        float SignedDistance(PointXY point);
    }
}
