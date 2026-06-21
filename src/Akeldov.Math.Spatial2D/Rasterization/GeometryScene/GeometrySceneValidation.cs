using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class GeometrySceneValidation
    {
        public static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }

        public static void ValidatePositiveFinite(float value, string parameterName, string message)
        {
            if (value <= 0f || float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidateNonNegativeFinite(float value, string parameterName, string message)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }
}
