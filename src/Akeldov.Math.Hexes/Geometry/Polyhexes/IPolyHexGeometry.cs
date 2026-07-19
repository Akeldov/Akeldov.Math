using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Describes a polyhex together with the physical dimensions of its regular hex cells.
    /// </summary>
    public interface IPolyhexGeometry : IPolyhex
    {
        /// <summary>
        /// Gets the distance from a hex center to an edge.
        /// </summary>
        float HexApothem { get; }

        /// <summary>
        /// Gets the distance from a hex center to a vertex.
        /// </summary>
        float HexRadius { get; }
    }
}
