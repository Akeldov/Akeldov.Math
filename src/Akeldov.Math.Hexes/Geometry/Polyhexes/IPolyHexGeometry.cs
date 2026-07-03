using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Defines a contract for IPolyhexGeometry implementations.
    /// </summary>
    public interface IPolyhexGeometry : IPolyhex
    {
        /// <summary>
        /// Represents the <c>HexApothem</c> value.
        /// </summary>
        float HexApothem { get; }

        /// <summary>
        /// Represents the <c>HexRadius</c> value.
        /// </summary>
        float HexRadius { get; }
    }
}
