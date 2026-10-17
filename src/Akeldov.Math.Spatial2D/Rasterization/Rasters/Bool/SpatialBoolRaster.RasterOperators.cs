using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class SpatialBoolRaster
    {
        /// <summary>Creates a spatial raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator &(SpatialBoolRaster left, BoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator &(BoolRaster left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator &(SpatialBoolRaster left, Raster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator &(Raster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator |(SpatialBoolRaster left, BoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator |(BoolRaster left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator |(SpatialBoolRaster left, Raster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator |(Raster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ^(SpatialBoolRaster left, BoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ^(BoolRaster left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ^(SpatialBoolRaster left, Raster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ^(Raster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ==(SpatialBoolRaster left, BoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ==(BoolRaster left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ==(SpatialBoolRaster left, Raster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator ==(Raster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator !=(SpatialBoolRaster left, BoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator !=(BoolRaster left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator !=(SpatialBoolRaster left, Raster<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(left.Geometry, values);
        }

        /// <summary>Creates a spatial raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller, with the spatial operand's geometry. Neither source is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static SpatialBoolRaster operator !=(Raster<bool> left, SpatialBoolRaster right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Resolution != right.Resolution)
                throw new ArgumentException("Rasters must have the same resolution.", nameof(right));

            var values = new bool[left.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolRaster(right.Geometry, values);
        }
    }
}
