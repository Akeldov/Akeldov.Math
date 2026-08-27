namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides read-only access to integer values arranged over a rectangular hex-map topology.
    /// </summary>
    public interface IIntHexMap : IHexMap<int>
    {
        /// <summary>
        /// Computes the minimum value in the map in O(N) time.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        int Min { get; }

        /// <summary>
        /// Computes the maximum value in the map in O(N) time.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the map contains no cells.</exception>
        int Max { get; }
    }
}
