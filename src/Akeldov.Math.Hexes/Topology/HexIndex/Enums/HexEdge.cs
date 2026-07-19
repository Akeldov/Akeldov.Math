namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies a hex edge by the direction from the cell center to the neighboring cell.
    /// Edge ordinals advance counterclockwise in 60-degree steps.
    /// </summary>
    public enum HexEdge
    {
        /// <summary>
        /// Direction 0: east in pointy-top layouts, northeast in flat-top layouts.
        /// </summary>
        Edge0 = 0,
        /// <summary>
        /// Direction 1: northeast in pointy-top layouts, north in flat-top layouts.
        /// </summary>
        Edge1 = 1,
        /// <summary>
        /// Direction 2: northwest in pointy-top layouts, northwest in flat-top layouts.
        /// </summary>
        Edge2 = 2,
        /// <summary>
        /// Direction 3: west in pointy-top layouts, southwest in flat-top layouts.
        /// </summary>
        Edge3 = 3,
        /// <summary>
        /// Direction 4: southwest in pointy-top layouts, south in flat-top layouts.
        /// </summary>
        Edge4 = 4,
        /// <summary>
        /// Direction 5: southeast in pointy-top layouts, southeast in flat-top layouts.
        /// </summary>
        Edge5 = 5
    }
}
