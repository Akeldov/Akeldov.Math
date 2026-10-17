using System;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>Stores a rectangular raster of Boolean values with raster-cell resolution but no spatial bounds.</summary>
    /// <remarks>
    /// Equality operators compare cells and return Boolean masks; they do not test object equality.
    /// Use pattern matching to test for null and <see cref="object.ReferenceEquals(object, object)"/> to compare references.
    /// </remarks>
    public partial class BoolRaster : Raster<bool>
    {
        /// <summary>Initializes a Boolean raster with the specified resolution and values.</summary>
        /// <param name="resolution">The raster resolution in cells. Both components must be positive and their product must fit in a one-dimensional array.</param>
        /// <param name="values">The row-major cell values, with X advancing first. The array must contain one value per raster cell.</param>
        /// <remarks>The array is retained without copying. The caller and raster share the same mutable storage.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when the values array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a resolution component is not positive or the cell count exceeds a 32-bit array length.</exception>
        /// <exception cref="ArgumentException">Thrown when the value count does not match the raster resolution.</exception>
        public BoolRaster(VectorXYInt resolution, bool[] values)
            : base(resolution, values)
        {
        }

        /// <summary>Creates a raster containing the logical negation of each source cell.</summary>
        /// <param name="raster">The source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static BoolRaster operator !(BoolRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = !raster[index];

            return new BoolRaster(raster.Resolution, values);
        }

        /// <summary>Creates a raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static BoolRaster operator &(BoolRaster left, BoolRaster right)
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

            return new BoolRaster(left.Resolution, values);
        }

        /// <summary>Creates a raster containing the disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static BoolRaster operator |(BoolRaster left, BoolRaster right)
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

            return new BoolRaster(left.Resolution, values);
        }

        /// <summary>Creates a raster containing the exclusive disjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static BoolRaster operator ^(BoolRaster left, BoolRaster right)
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

            return new BoolRaster(left.Resolution, values);
        }
    }
}
#pragma warning restore CS0660, CS0661

