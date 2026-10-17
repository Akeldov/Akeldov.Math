using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class SpatialBoolRaster
    {
        /// <summary>Creates a raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different geometries.</exception>
        public static SpatialBoolRaster operator ==(SpatialBoolRaster left, SpatialBoolRaster right)
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
        public static SpatialBoolRaster operator !=(SpatialBoolRaster left, SpatialBoolRaster right)
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

        /// <summary>Creates a Boolean mask of cells equal to the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static SpatialBoolRaster operator ==(SpatialBoolRaster raster, bool value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] == value;

            return new SpatialBoolRaster(raster.Geometry, values);
        }

        /// <summary>Creates a Boolean mask of cells equal to the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static SpatialBoolRaster operator ==(bool value, SpatialBoolRaster raster) => raster == value;

        /// <summary>Creates a Boolean mask of cells different from the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static SpatialBoolRaster operator !=(SpatialBoolRaster raster, bool value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] != value;

            return new SpatialBoolRaster(raster.Geometry, values);
        }

        /// <summary>Creates a Boolean mask of cells different from the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source geometry is preserved. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static SpatialBoolRaster operator !=(bool value, SpatialBoolRaster raster) => raster != value;
    }
}
