using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines a contract for IHexMap implementations.
    /// </summary>
    /// <typeparam name="TValue">The type of value handled by this member.</typeparam>
    public interface IHexMap<TValue>
    {
        /// <summary>
        /// Gets the map topology.
        /// </summary>
        HexMapTopology Topology { get; }

        /// <summary>
        /// Represents the <c>this[VectorXYInt]</c> value.
        /// </summary>
        /// <param name="index">The index value.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Gets the value at the specified flat index. Flat indexes use row-major order: X advances
        /// first, and coordinates <c>(x, y)</c> map to <c>y * Topology.Resolution.X + x</c>.
        /// </summary>
        /// <param name="index">The index value.</param>
        TValue this[int index] { get; }
    }
}
