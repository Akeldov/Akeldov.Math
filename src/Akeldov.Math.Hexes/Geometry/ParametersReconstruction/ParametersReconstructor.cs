using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Represents a ParametersReconstructor instance.
    /// </summary>
    public static class ParametersReconstructor
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="size">The size value.</param>
        /// <param name="dim">The dim value.</param>
        /// <param name="xOriented">The xOriented value.</param>
        public static float GetApothem(VectorXY size, VectorXYInt dim, bool xOriented)
        {
            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size components must be finite and positive.");

            if (dim.X <= 0 || dim.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(dim), dim, "Dimension components must be positive.");

            double apothem = xOriented
                ? dim.Y == 1
                    ? (double)size.X / dim.X / 2d
                    : (double)size.X / ((double)dim.X * 2d + 1d)
                : dim.X == 1
                    ? (double)size.Y / dim.Y / 2d
                    : (double)size.Y / ((double)dim.Y * 2d + 1d);

            if (double.IsNaN(apothem) || double.IsInfinity(apothem) ||
                apothem < float.Epsilon || apothem > float.MaxValue)
                throw new OverflowException("Reconstructed hex apothem must fit in a finite positive Single.");

            return (float)apothem;
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="landscapeMetricSize">The landscapeMetricSize value.</param>
        /// <param name="hexApothem">The hexApothem value.</param>
        /// <param name="xOrientation">The xOrientation value.</param>
        public static VectorXYInt GetDim(VectorXY landscapeMetricSize, float hexApothem, bool xOrientation)
        {
            if (!landscapeMetricSize.IsFinite || landscapeMetricSize.X < 0f || landscapeMetricSize.Y < 0f)
                throw new ArgumentOutOfRangeException(nameof(landscapeMetricSize), landscapeMetricSize, "Size components must be finite and non-negative.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            double apothem = hexApothem;
            double hexRadius = (double)Constants.Apothem2Radius * hexApothem;

            double xHexCount = 0d;
            double yHexCount = 0d;
            double sizeX = landscapeMetricSize.X;
            double sizeY = landscapeMetricSize.Y;

            if (xOrientation)
            {
                if (sizeX < apothem * 2d || sizeY < hexRadius * 2d)
                {
                    xHexCount = 0d;
                    yHexCount = 0d;
                }
                else if (sizeY < hexRadius * 3.5d)
                {
                    xHexCount = sizeX / (apothem * 2d);
                    yHexCount = 1d;
                }
                else
                {
                    xHexCount = (sizeX - apothem) / (apothem * 2d);
                    yHexCount = (sizeY - hexRadius * 2d) / (hexRadius * 1.5d) + 1d;
                }
            }
            else
            {
                if (sizeY < apothem * 2d || sizeX < hexRadius * 2d)
                {
                    xHexCount = 0d;
                    yHexCount = 0d;
                }
                else if (sizeX < hexRadius * 3.5d)
                {
                    xHexCount = 1d;
                    yHexCount = sizeY / (apothem * 2d);
                }
                else
                {
                    xHexCount = (sizeX - hexRadius * 2d) / (hexRadius * 1.5d) + 1d;
                    yHexCount = (sizeY - apothem) / (apothem * 2d);
                }
            }

            int xHexSize = ConvertHexCountToInt32(xHexCount);
            int yHexSize = ConvertHexCountToInt32(yHexCount);

            var landscapeHexSize = new VectorXYInt(xHexSize, yHexSize);
            return landscapeHexSize;
        }

        private static int ConvertHexCountToInt32(double hexCount)
        {
            if (double.IsNaN(hexCount) || double.IsInfinity(hexCount) || hexCount > int.MaxValue)
                throw new OverflowException("Reconstructed hex dimension must fit in Int32.");

            return hexCount <= 0d ? 0 : (int)hexCount;
        }
    }
}
