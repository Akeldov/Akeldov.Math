namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents geometry that can count fill-rule crossings of a horizontal rightward ray.
    /// </summary>
    public interface IRightwardCrossingProvider
    {
        /// <summary>
        /// Counts intersections with the open horizontal ray extending rightward from the specified origin.
        /// </summary>
        /// <param name="origin">The finite origin of the horizontal ray.</param>
        /// <returns>The non-negative number of rightward crossings.</returns>
        /// <remarks>
        /// Crossings use a half-open vertical interval: a curve endpoint that is lower than its adjacent
        /// curve portion is included, while an upper endpoint is excluded. Horizontal portions do not count.
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="origin"/> has a non-finite coordinate.</exception>
        int CountRightwardCrossings(PointXY origin);
    }
}
