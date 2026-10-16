using System;
using System.Diagnostics.CodeAnalysis;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of integer values sampled on a <see cref="RasterGeometry"/>.
    /// </summary>
    public partial class SpatialIntRaster : IntRaster, ISpatialRaster<int>
    {
        /// <summary>
        /// Initializes a new integer raster with the specified geometry and values.
        /// </summary>
        /// <param name="geometry">The valid raster geometry. Its resolution cell count must fit in a one-dimensional array.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> is invalid or the raster cell count exceeds a 32-bit array length.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the value count does not match the raster geometry resolution.</exception>
        public SpatialIntRaster(RasterGeometry geometry, int[] values)
            : base(ValidateAndGetResolution(geometry, values), values)
        {
            Geometry = geometry;
        }

        /// <summary>
        /// Gets the raster geometry. Its resolution equals <see cref="Raster{TValue}.Resolution"/>.
        /// </summary>
        public RasterGeometry Geometry { get; }

        /// <summary>
        /// Creates a generic spatial raster sharing the source geometry and backing array.
        /// </summary>
        /// <param name="raster">The source raster, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable raster wrapper sharing the source values, or <see langword="null"/> if
        /// <paramref name="raster"/> is null. Changes through either raster are visible through the other.
        /// </returns>
        /// <remarks>The conversion allocates only the wrapper object. The value array is not copied.</remarks>
        [return: NotNullIfNotNull("raster")]
        public static implicit operator SpatialRaster<int>?(SpatialIntRaster? raster)
            => raster is null ? null : new SpatialRaster<int>(raster.Geometry, raster.Values);

        /// <summary>
        /// Creates a raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialIntRaster left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialIntRaster raster, int value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new int[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(raster[index] + value);

            return new SpatialIntRaster(raster.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(int value, SpatialIntRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new int[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value + raster[index]);

            return new SpatialIntRaster(raster.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster raster, int value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new int[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(raster[index] - value);

            return new SpatialIntRaster(raster.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of each source cell and a scalar in operand order.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The scalar value.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(int value, SpatialIntRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new int[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value - raster[index]);

            return new SpatialIntRaster(raster.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the negated source cell values.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <returns>A new mutable raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new int[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(-raster[index]);

            return new SpatialIntRaster(raster.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialIntRaster left, SpatialRaster<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialRaster<int> left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster left, SpatialRaster<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialRaster<int> left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        private static VectorXYInt ValidateAndGetResolution(RasterGeometry geometry, int[] values)
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
