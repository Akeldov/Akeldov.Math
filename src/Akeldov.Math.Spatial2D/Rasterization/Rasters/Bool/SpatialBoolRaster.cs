using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of Boolean values sampled on a <see cref="RasterGeometry"/>.
    /// </summary>
    /// <remarks>
    /// Equality operators return per-cell masks rather than object equality.
    /// Spatial operands must have the same geometry. Overloads accepting non-spatial operands
    /// compare resolutions only and retain the spatial operand's geometry.
    /// </remarks>
    public partial class SpatialBoolRaster : BoolRaster, ISpatialRaster<bool>
    {
        /// <summary>
        /// Initializes a new Boolean raster with the specified geometry and values.
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
        public SpatialBoolRaster(RasterGeometry geometry, bool[] values)
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
        public static implicit operator SpatialRaster<bool>?(SpatialBoolRaster? raster)
            => raster is null ? null : new SpatialRaster<bool>(raster.Geometry, raster.Values);

        /// <summary>Creates a raster containing the logical negation of each source cell.</summary>
        /// <param name="raster">The source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static SpatialBoolRaster operator !(SpatialBoolRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = !raster[index];

            return new SpatialBoolRaster(raster.Geometry, values);
        }

        /// <summary>Creates a raster containing the conjunction of corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator &(SpatialBoolRaster left, SpatialBoolRaster right)
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
        public static SpatialBoolRaster operator |(SpatialBoolRaster left, SpatialBoolRaster right)
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
        public static SpatialBoolRaster operator ^(SpatialBoolRaster left, SpatialBoolRaster right)
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

        private static VectorXYInt ValidateAndGetResolution(RasterGeometry geometry, bool[] values)
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
#pragma warning restore CS0660, CS0661
