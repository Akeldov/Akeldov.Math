using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a filled two-dimensional region defined by one or more contours.
    /// </summary>
    public interface IRegion
    {
        /// <summary>
        /// Gets the read-only structural view of the contours that define this region.
        /// </summary>
        IReadOnlyList<IContour> Contours { get; }

        /// <summary>
        /// Gets the fill rule used to interpret the contours.
        /// </summary>
        FillRule FillRule { get; }

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
