namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Controls handling of disconnected components of a hex Voronoi cell.
    /// </summary>
    public enum ExclavePolicy
    {
        /// <summary>Preserves the original assignments, including disconnected components.</summary>
        LeaveAsIs = 0,

        /// <summary>
        /// Keeps each cell's six-connected component containing its closest hex to the site.
        /// Reassigns other components in simultaneous layers, growing from retained components.
        /// Each hex selects the adjacent grown cell with the closest site by unweighted Euclidean
        /// distance; ties favor the earlier source site. Region boundaries and excluded hexes
        /// are preserved. Each component unreachable from a retained component becomes a new cell,
        /// appended after the source cells. Its site is the component hex closest to the original
        /// site (row-major order breaks ties), with the original site weight.
        /// </summary>
        ReassignToClosestCell = 1
    }
}
