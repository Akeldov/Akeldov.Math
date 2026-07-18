using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of values sampled on a <see cref="RasterGeometry"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type stored in each raster cell.</typeparam>
    public class SpatialRaster<TValue> : Raster<TValue>, ISpatialRaster<TValue>
    {
        /// <summary>
        /// Initializes a new raster with the specified geometry and values.
        /// </summary>
        /// <param name="geometry">The valid raster geometry. Its resolution cell count must fit in a one-dimensional array.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> is invalid or its raster cell count exceeds a 32-bit array length.
        /// </exception>
        public SpatialRaster(RasterGeometry geometry, TValue[] values)
            : base(ValidateAndGetResolution(geometry, values), values)
        {
            Geometry = geometry;
        }

        /// <summary>
        /// Gets the raster geometry.
        /// </summary>
        public RasterGeometry Geometry { get; }

        private static VectorXYInt ValidateAndGetResolution(RasterGeometry geometry, TValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (float.IsNaN(geometry.Origin.X) || float.IsInfinity(geometry.Origin.X) ||
                float.IsNaN(geometry.Origin.Y) || float.IsInfinity(geometry.Origin.Y) ||
                float.IsNaN(geometry.Size.X) || float.IsInfinity(geometry.Size.X) || geometry.Size.X <= 0f ||
                float.IsNaN(geometry.Size.Y) || float.IsInfinity(geometry.Size.Y) || geometry.Size.Y <= 0f ||
                geometry.Resolution.X <= 0 || geometry.Resolution.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(geometry),
                    geometry,
                    "Raster geometry must have finite bounds, positive size, and positive resolution components.");
            }

            long cellCount = (long)geometry.Resolution.X * geometry.Resolution.Y;
            if (cellCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(geometry),
                    geometry,
                    "Raster cell count must fit in a one-dimensional array.");

            if (values.Length != (int)cellCount)
                throw new ArgumentException("Raster value count must match the raster geometry resolution.", nameof(values));

            return geometry.Resolution;
        }
    }
}
