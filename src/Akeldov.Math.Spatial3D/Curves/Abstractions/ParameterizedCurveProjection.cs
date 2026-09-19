using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents the result of projecting a point onto a parameterized three-dimensional curve.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ParameterizedCurveProjection
    {
        /// <summary>
        /// Initializes a new parameterized projection result.
        /// </summary>
        /// <param name="projectedPoint">The finite projected point on the curve.</param>
        /// <param name="curveCoordinate">
        /// The curve coordinate of the projected point in world coordinate units. Infinity is allowed; NaN is not.
        /// </param>
        /// <param name="distance">The finite, non-negative distance from the original point to the projected point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="projectedPoint"/> has a non-finite coordinate,
        /// <paramref name="curveCoordinate"/> is NaN, or <paramref name="distance"/> is negative, NaN, or infinite.
        /// </exception>
        public ParameterizedCurveProjection(PointXYZ projectedPoint, float curveCoordinate, float distance)
        {
            if (float.IsNaN(projectedPoint.X) || float.IsInfinity(projectedPoint.X) ||
                float.IsNaN(projectedPoint.Y) || float.IsInfinity(projectedPoint.Y) ||
                float.IsNaN(projectedPoint.Z) || float.IsInfinity(projectedPoint.Z))
                throw new ArgumentOutOfRangeException(nameof(projectedPoint), "Projected point coordinates must be finite.");

            if (float.IsNaN(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must not be NaN.");

            if (distance < 0f || float.IsNaN(distance) || float.IsInfinity(distance))
                throw new ArgumentOutOfRangeException(nameof(distance), "Projection distance must be finite and non-negative.");

            ProjectedPoint = projectedPoint;
            CurveCoordinate = curveCoordinate;
            Distance = distance;
        }

        /// <summary>
        /// Gets the projected point on the curve.
        /// </summary>
        public PointXYZ ProjectedPoint { get; }

        /// <summary>
        /// Gets the curve coordinate of the projected point in world coordinate units.
        /// </summary>
        /// <remarks>
        /// Uses the coordinate system defined by <see cref="IParameterizedCurve.GetPoint"/>.
        /// This value is not normalized to the <c>[0, 1]</c> range and may be infinite.
        /// </remarks>
        public float CurveCoordinate { get; }

        /// <summary>
        /// Gets the distance from the original point to the projected point.
        /// </summary>
        public float Distance { get; }
    }
}
