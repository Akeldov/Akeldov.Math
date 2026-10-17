using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class SpatialBoolRaster
    {
        /// <summary>Creates a raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator &(SpatialBoolRaster left, SpatialRaster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator &(SpatialRaster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator |(SpatialBoolRaster left, SpatialRaster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator |(SpatialRaster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator ^(SpatialBoolRaster left, SpatialRaster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator ^(SpatialRaster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator ==(SpatialBoolRaster left, SpatialRaster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator ==(SpatialRaster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator !=(SpatialBoolRaster left, SpatialRaster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator !=(SpatialRaster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial rasters must have the same geometry.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }
    }
}
