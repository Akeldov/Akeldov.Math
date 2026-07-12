using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines a hex map whose cells have a spatial geometry.
    /// </summary>
    /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
    public interface ISpatialHexMap<TValue> : IHexMap<TValue>
    {
        /// <summary>
        /// Gets the spatial geometry of the hex map.
        /// </summary>
        HexMapGeometry Geometry { get; }
    }
}
