using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class RasterGeometryValidation
    {
        internal static int ValidateAndGetCellCount(RasterGeometry geometry, string parameterName)
        {
            if (float.IsNaN(geometry.Origin.X) || float.IsInfinity(geometry.Origin.X) ||
                float.IsNaN(geometry.Origin.Y) || float.IsInfinity(geometry.Origin.Y) ||
                !geometry.Size.IsFinite || geometry.Size.X <= 0f || geometry.Size.Y <= 0f ||
                geometry.Resolution.X <= 0 || geometry.Resolution.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    geometry,
                    "Raster geometry must have finite bounds, positive size, and positive resolution components.");
            }

            long cellCount = (long)geometry.Resolution.X * geometry.Resolution.Y;
            if (cellCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    geometry,
                    "Raster cell count must fit in a one-dimensional array.");
            }

            return (int)cellCount;
        }
    }
}
