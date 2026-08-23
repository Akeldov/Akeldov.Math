using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour.
    /// </summary>
    public interface IContour : IFiniteCurve, ISignedPointDistanceProvider, IRightwardCrossingProvider, IRayIntersectionProvider
    {
        /// <summary>
        /// Determines whether the specified point lies inside or on this closed contour.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns><see langword="true"/> if the point lies inside or on the closed contour; otherwise, <see langword="false"/>.</returns>
        bool Encloses(PointXY point);
    }
}
