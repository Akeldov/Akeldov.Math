using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour made from finite paths.
    /// </summary>
    public interface IContour
    {
        /// <summary>
        /// Gets the read-only structural view of the finite paths that form this contour.
        /// </summary>
        IReadOnlyList<IFinitePath> Curves { get; }

        /// <summary>
        /// Determines whether the specified point lies inside or on this closed contour.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns><see langword="true"/> if the point lies inside or on the closed contour; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        bool Encloses(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon);

        /// <summary>
        /// Returns the shortest unsigned distance from the specified point to a contour boundary.
        /// </summary>
        /// <param name="point">The finite point to measure from.</param>
        /// <returns>The shortest non-negative distance to the contour boundary in world coordinate units.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> has a non-finite coordinate.</exception>
        float Distance(PointXY point);

        /// <summary>
        /// Returns the signed distance from the specified point to this contour boundary.
        /// </summary>
        /// <param name="point">The finite point to measure from.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>
        /// The shortest distance to the contour boundary, negated when <paramref name="point"/> lies inside or on this contour.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has a non-finite coordinate, or when
        /// <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon);
    }
}
