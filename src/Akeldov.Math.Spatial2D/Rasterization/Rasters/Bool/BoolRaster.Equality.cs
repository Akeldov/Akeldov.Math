using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public partial class BoolRaster
    {
        /// <summary>Creates a raster containing a mask of equal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static BoolRaster operator ==(BoolRaster left, BoolRaster right)
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

            return new BoolRaster(left.Resolution, values);
        }

        /// <summary>Creates a raster containing a mask of unequal corresponding source cells.</summary>
        /// <param name="left">The left source raster.</param>
        /// <param name="right">The right source raster.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. Neither source raster is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source raster is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the source rasters have different resolutions.</exception>
        public static BoolRaster operator !=(BoolRaster left, BoolRaster right)
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

            return new BoolRaster(left.Resolution, values);
        }

        /// <summary>Creates a Boolean mask of cells equal to the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static BoolRaster operator ==(BoolRaster raster, bool value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] == value;

            return new BoolRaster(raster.Resolution, values);
        }

        /// <summary>Creates a Boolean mask of cells equal to the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static BoolRaster operator ==(bool value, BoolRaster raster) => raster == value;

        /// <summary>Creates a Boolean mask of cells different from the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static BoolRaster operator !=(BoolRaster raster, bool value)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new bool[raster.Values.Length];
            for (int index = 0; index < values.Length; index++)
                values[index] = raster[index] != value;

            return new BoolRaster(raster.Resolution, values);
        }

        /// <summary>Creates a Boolean mask of cells different from the specified value.</summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="value">The value compared with each cell.</param>
        /// <returns>A new mutable Boolean raster owned by the caller. The source raster is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source raster is null.</exception>
        public static BoolRaster operator !=(bool value, BoolRaster raster) => raster != value;
    }
}
