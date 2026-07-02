using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static class ParametersReconstructor
    {
        public static float GetApothem(VectorXY size, VectorXYInt dim, bool xOriented)
        {
            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size components must be finite and positive.");

            if (dim.X <= 0 || dim.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(dim), dim, "Dimension components must be positive.");

            float apothem = xOriented
                ? dim.Y == 1
                    ? size.X / dim.X / 2
                    : size.X / (dim.X * 2 + 1)
                : dim.X == 1
                    ? size.Y / dim.Y / 2
                    : size.Y / (dim.Y * 2 + 1);

            return apothem;
        }

        public static VectorXYInt GetDim(VectorXY landscapeMetricSize, float hexApothem, bool xOrientation)
        {
            if (!landscapeMetricSize.IsFinite || landscapeMetricSize.X < 0f || landscapeMetricSize.Y < 0f)
                throw new ArgumentOutOfRangeException(nameof(landscapeMetricSize), landscapeMetricSize, "Size components must be finite and non-negative.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            var hexRadius = hexApothem.ConvertHexApothemToRadius();

            float xHexCount = 0;
            float yHexCount = 0;

            if (xOrientation)
            {
                if (landscapeMetricSize.X < hexApothem * 2f || landscapeMetricSize.Y < hexRadius * 2f)
                {
                    xHexCount = 0;
                    yHexCount = 0;
                }
                else if (landscapeMetricSize.Y < hexRadius * 3.5f)
                {
                    xHexCount = landscapeMetricSize.X / (hexApothem * 2);
                    yHexCount = 1;
                }
                else
                {
                    xHexCount = (landscapeMetricSize.X - hexApothem) / (hexApothem * 2);
                    yHexCount = (landscapeMetricSize.Y - hexRadius * 2) / (hexRadius * 1.5f) + 1;
                }
            }
            else
            {
                if (landscapeMetricSize.Y < hexApothem * 2f || landscapeMetricSize.X < hexRadius * 2f)
                {
                    xHexCount = 0;
                    yHexCount = 0;
                }
                else if (landscapeMetricSize.X < hexRadius * 3.5f)
                {
                    xHexCount = 1;
                    yHexCount = landscapeMetricSize.Y / (hexApothem * 2);
                }
                else
                {
                    xHexCount = (landscapeMetricSize.X - hexRadius * 2) / (hexRadius * 1.5f) + 1;
                    yHexCount = (landscapeMetricSize.Y - hexApothem) / (hexApothem * 2);
                }
            }

            int xHexSize = (int)xHexCount;
            int yHexSize = (int)yHexCount;

            var landscapeHexSize = new VectorXYInt(xHexSize, yHexSize);
            return landscapeHexSize;
        }
    }
}
