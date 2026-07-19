namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies a hex vertex in counterclockwise order.
    /// </summary>
    public enum HexVertex
    {
        /// <summary>
        /// The vertex at 30 degrees for pointy-top hexes or 0 degrees for flat-top hexes.
        /// </summary>
        Vertex0 = 0,
        /// <summary>
        /// The vertex 60 degrees counterclockwise from <see cref="Vertex0"/>.
        /// </summary>
        Vertex1 = 1,
        /// <summary>
        /// The vertex 120 degrees counterclockwise from <see cref="Vertex0"/>.
        /// </summary>
        Vertex2 = 2,
        /// <summary>
        /// The vertex 180 degrees counterclockwise from <see cref="Vertex0"/>.
        /// </summary>
        Vertex3 = 3,
        /// <summary>
        /// The vertex 240 degrees counterclockwise from <see cref="Vertex0"/>.
        /// </summary>
        Vertex4 = 4,
        /// <summary>
        /// The vertex 300 degrees counterclockwise from <see cref="Vertex0"/>.
        /// </summary>
        Vertex5 = 5
    }
}
