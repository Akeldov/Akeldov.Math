using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents the result of projecting a point onto a three-dimensional curve.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CurveProjection
    {
        /// <summary>
        /// Initializes a new projection result.
        /// </summary>
        /// <param name="projectedPoint">The finite projected point on the curve.</param>
        /// <param name="distance">The finite, non-negative distance from the original point to the projected point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="projectedPoint"/> has a non-finite coordinate,
        /// or <paramref name="distance"/> is negative, NaN, or infinite.
        /// </exception>
        public CurveProjection(PointXYZ projectedPoint, float distance)
        {
            if (float.IsNaN(projectedPoint.X) || float.IsInfinity(projectedPoint.X) ||
                float.IsNaN(projectedPoint.Y) || float.IsInfinity(projectedPoint.Y) ||
                float.IsNaN(projectedPoint.Z) || float.IsInfinity(projectedPoint.Z))
                throw new ArgumentOutOfRangeException(nameof(projectedPoint), "Projected point coordinates must be finite.");

            if (distance < 0f || float.IsNaN(distance) || float.IsInfinity(distance))
                throw new ArgumentOutOfRangeException(nameof(distance), "Projection distance must be finite and non-negative.");

            ProjectedPoint = projectedPoint;
            Distance = distance;
        }

        /// <summary>
        /// Gets the projected point on the curve.
        /// </summary>
        public PointXYZ ProjectedPoint { get; }

        /// <summary>
        /// Gets the distance from the original point to the projected point.
        /// </summary>
        public float Distance { get; }
    }
}
