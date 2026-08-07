namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides read-only access to integer values arranged over a rectangular hex-map topology.
    /// </summary>
    public interface IIntHexMap : IHexMap<int>
    {
        /// <summary>
        /// Gets the minimum value in the map.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        int Min { get; }

        /// <summary>
        /// Gets the maximum value in the map.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        int Max { get; }
    }
}
