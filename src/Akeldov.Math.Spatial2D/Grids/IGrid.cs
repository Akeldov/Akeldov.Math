namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a rectangular grid of values.
    /// </summary>
    /// <typeparam name="TValue">The value type returned by the grid.</typeparam>
    public interface IGrid<out TValue>
    {
        /// <summary>
        /// Gets the number of grid columns.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the number of grid rows.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the value at the specified two-dimensional grid index.
        /// </summary>
        /// <param name="index">The zero-based grid index. X selects the column, and Y selects the row.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Gets the value at the specified two-dimensional grid index.
        /// </summary>
        /// <param name="x">The zero-based column index.</param>
        /// <param name="y">The zero-based row index.</param>
        TValue this[int x, int y] { get; }

        /// <summary>
        /// Gets the value at the specified flat grid index.
        /// </summary>
        /// <param name="index">The zero-based flat grid index in the implementation-defined ordering.</param>
        TValue this[int index] { get; }
    }
}
