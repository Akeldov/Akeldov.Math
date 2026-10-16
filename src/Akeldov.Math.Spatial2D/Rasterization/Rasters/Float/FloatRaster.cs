using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of floating-point values with raster-cell resolution but no spatial bounds.
    /// </summary>
    public class FloatRaster : Raster<float>
    {
        /// <summary>
        /// Initializes a new floating-point raster with the specified resolution and values.
        /// </summary>
        /// <param name="resolution">The raster resolution in cells. Both components must be positive and their product must fit in a one-dimensional array.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a resolution component is not positive or the raster cell count exceeds a 32-bit array length.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the value count does not match the raster resolution.</exception>
        public FloatRaster(VectorXYInt resolution, float[] values)
            : base(resolution, values)
        {
        }
    }
}
