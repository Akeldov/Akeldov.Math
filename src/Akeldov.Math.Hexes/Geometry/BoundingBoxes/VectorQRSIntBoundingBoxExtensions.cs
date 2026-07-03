using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VectorQRSIntExtensions
    {
        /// <summary>
        /// Performs the BoundingBoxSize operation.
        /// </summary>
        /// <param name="size">The size value.</param>
        /// <param name="apothem">The apothem value.</param>
        /// <param name="radius">The radius value.</param>
        /// <param name="layout">The layout value.</param>
        public static VectorXY BoundingBoxSize(this VectorQRSInt size, float apothem, float radius, Layout layout)
        {
            if (size.Q < 0 || size.R < 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Cannot calculate bounding box for size with negative components.");

            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            if (size.Q == 0 || size.R == 0)
                return new VectorXY(0, 0);

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorXY(
                        apothem * (2f * size.Q + size.R - 1),
                        radius * (1.5f * size.R + 0.5f));
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorXY(
                        radius * (1.5f * size.Q + 0.5f),
                        apothem * (2f * size.R + size.Q - 1));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
