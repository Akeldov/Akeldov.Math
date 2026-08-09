namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Defines how a Voronoi partitioner handles cells that receive no items.
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
