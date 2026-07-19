using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides read-only access to values arranged over a rectangular hex-map topology.
    /// </summary>
    /// <typeparam name="TValue">The type stored in each hex cell.</typeparam>
    public interface IHexMap<TValue>
    {
        /// <summary>
        /// Gets the map topology.
        /// </summary>
        HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the value at the specified offset-grid coordinates.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Gets the value at the specified flat index. Flat indexes use row-major order: X advances
        /// first, and coordinates <c>(x, y)</c> map to <c>y * Topology.Resolution.X + x</c>.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        TValue this[int index] { get; }
    }
}
