namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a two-dimensional curve that can measure distances to points and project points onto itself.
    /// </summary>
    public interface ICurve : IPointDistanceProvider
    {
        /// <summary>
        /// Projects the specified point onto this curve.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The projection point and distance to this curve.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="point"/> has a non-finite coordinate.</exception>
        CurveProjection Project(PointXY point);
    }
}
