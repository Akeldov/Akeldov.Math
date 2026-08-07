namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides read-only access to floating-point values arranged over a rectangular hex-map topology.
    /// </summary>
    public interface IFloatHexMap : IHexMap<float>
    {
        /// <summary>
        /// Gets the minimum value in the map.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        float Min { get; }

        /// <summary>
        /// Gets the maximum value in the map.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        float Max { get; }
    }
}
