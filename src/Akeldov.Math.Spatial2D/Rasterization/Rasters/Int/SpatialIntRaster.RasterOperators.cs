using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class SpatialIntRaster
    {
        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialIntRaster left, IntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(IntRaster left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster left, IntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(IntRaster left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(SpatialIntRaster left, Raster<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator +(Raster<int> left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(SpatialIntRaster left, Raster<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        /// <exception cref="OverflowException">Thrown when a cell result does not fit in <see cref="int"/>.</exception>
        public static SpatialIntRaster operator -(Raster<int> left, SpatialIntRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new int[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntRaster(right.Geometry, values);
        }
    }
}
