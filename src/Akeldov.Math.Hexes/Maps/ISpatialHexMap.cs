using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines a hex map whose cells have a spatial geometry.
    /// </summary>
    /// <remarks>
    /// Implementations must maintain <c>Geometry.Topology == Topology</c> for the lifetime of the map.
    /// Like <see cref="IHexMap{TValue}"/>, this interface is a read-only view and does not guarantee
    /// immutable or snapshot values.
    /// </remarks>
    /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
    public interface ISpatialHexMap<TValue> : IHexMap<TValue>
    {
        /// <summary>
        /// Gets the spatial geometry of the hex map. Its topology must equal <see cref="IHexMap{TValue}.Topology"/>.
        /// </summary>
        HexMapGeometry Geometry { get; }
    }
}
