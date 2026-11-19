namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Defines how to handle empty cells in the final Voronoi partition.
    /// An item partition is empty when it has no items; a geometric cell is empty when it has no area.
    /// </summary>
    public enum EmptyCellPolicy
    {
        /// <summary>
        /// Throws an exception when any generated cell is empty.
        /// </summary>
        ThrowException = 0,

        /// <summary>
        /// Removes empty cells from the partitioning result.
        /// </summary>
        Exclude = 1,

        /// <summary>
        /// Keeps empty cells in the partitioning result.
        /// </summary>
        LeaveAsIs = 2
    }
}
