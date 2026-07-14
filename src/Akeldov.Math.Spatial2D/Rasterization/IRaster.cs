namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a rectangular raster of values.
    /// </summary>
    /// <typeparam name="TValue">The value type returned by the grid.</typeparam>
    public interface IRaster<out TValue>
    {
        /// <summary>
        /// Gets the raster resolution in columns and rows.
        /// </summary>
        VectorXYInt Resolution { get; }

        /// <summary>
        /// Gets the value at the specified two-dimensional raster index.
        /// </summary>
        /// <param name="index">The zero-based raster index. X selects the column, and Y selects the row.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Gets the value at the specified two-dimensional raster index.
        /// </summary>
        /// <param name="x">The zero-based column index.</param>
        /// <param name="y">The zero-based row index.</param>
        TValue this[int x, int y] { get; }

        /// <summary>
        /// Gets the value at the specified flat raster index.
        /// </summary>
        /// <param name="index">The zero-based flat raster index in the implementation-defined ordering.</param>
        TValue this[int index] { get; }
    }
}
