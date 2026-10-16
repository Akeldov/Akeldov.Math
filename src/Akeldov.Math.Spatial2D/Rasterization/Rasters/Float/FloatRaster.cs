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

        /// <summary>
        /// Creates a raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static FloatRaster operator +(FloatRaster left, FloatRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new float[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new FloatRaster(left.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static FloatRaster operator +(FloatRaster raster, float value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new float[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] + value;

            return new FloatRaster(raster.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static FloatRaster operator +(float value, FloatRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new float[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = value + raster[index];

            return new FloatRaster(raster.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static FloatRaster operator -(FloatRaster left, FloatRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new float[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new FloatRaster(left.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static FloatRaster operator -(FloatRaster raster, float value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new float[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] - value;

            return new FloatRaster(raster.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static FloatRaster operator -(float value, FloatRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new float[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = value - raster[index];

            return new FloatRaster(raster.Resolution, values);
        }

        /// <summary>
        /// Creates a raster containing the negated source cell values.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static FloatRaster operator -(FloatRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new float[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = -raster[index];

            return new FloatRaster(raster.Resolution, values);
        }

    }
}
