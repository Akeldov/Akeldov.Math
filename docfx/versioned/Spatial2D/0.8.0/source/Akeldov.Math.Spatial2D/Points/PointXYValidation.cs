using System;

namespace Akeldov.Math.Spatial2D
{
    internal static class PointXYValidation
    {
        public static void ThrowIfNotFinite(PointXY point, string parameterName, string message)
        {
            if (float.IsNaN(point.X) || float.IsNaN(point.Y) ||
                float.IsInfinity(point.X) || float.IsInfinity(point.Y))
            {
                throw new ArgumentOutOfRangeException(parameterName, message);
            }
        }

        public static bool IsFinite(PointXY point)
        {
            return !float.IsNaN(point.X) && !float.IsNaN(point.Y) &&
                !float.IsInfinity(point.X) && !float.IsInfinity(point.Y);
        }
    }
}
