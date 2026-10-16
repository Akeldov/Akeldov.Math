using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class SpatialFloatRaster
    {
        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator +(SpatialFloatRaster left, FloatRaster right)
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

            return new SpatialFloatRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator +(FloatRaster left, SpatialFloatRaster right)
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

            return new SpatialFloatRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator -(SpatialFloatRaster left, FloatRaster right)
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

            return new SpatialFloatRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator -(FloatRaster left, SpatialFloatRaster right)
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

            return new SpatialFloatRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator +(SpatialFloatRaster left, Raster<float> right)
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

            return new SpatialFloatRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the sums of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator +(Raster<float> left, SpatialFloatRaster right)
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

            return new SpatialFloatRaster(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator -(SpatialFloatRaster left, Raster<float> right)
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

            return new SpatialFloatRaster(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial raster containing the differences of corresponding source cells.
        /// </summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialFloatRaster operator -(Raster<float> left, SpatialFloatRaster right)
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

            return new SpatialFloatRaster(right.Geometry, values);
        }
    }
}
