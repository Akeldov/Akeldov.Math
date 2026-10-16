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

            int cellCount = RasterGeometryValidation.ValidateAndGetCellCount(geometry, nameof(geometry));

            if (values.Length != cellCount)
                throw new ArgumentException("Raster value count must match the raster geometry resolution.", nameof(values));

            return geometry.Resolution;
        }
    }
}
