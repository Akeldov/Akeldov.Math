using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a rectangular grid with a value stored at each grid node.
    /// A grid node does not represent a hex by itself; usually several grid nodes
    /// may correspond to one hex. Values in the same row or column lie on a straight
    /// line geometrically, although a grid implementation may rely only on topology
    /// and not on geometric coordinates.
    /// </summary>
    /// <typeparam name="TValue">The value type stored in grid nodes.</typeparam>
    public interface IGrid<TValue>
    {
        /// <summary>
        /// Gets the grid width in nodes.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the grid height in nodes.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the value stored at the specified two-dimensional grid node index.
        /// </summary>
        /// <param name="index">The zero-based grid node index.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Gets the value stored at the specified flat grid node index.
        /// </summary>
        /// <param name="index">The zero-based flat grid node index.</param>
        TValue this[int index] { get; }
    }
}
