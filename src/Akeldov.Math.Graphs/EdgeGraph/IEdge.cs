namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines an edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    public interface IEdge<TVertex, TEdge>
        where TVertex : IEdgeGraphVertex<TVertex, TEdge>
        where TEdge : IEdge<TVertex, TEdge>
    {
        /// <summary>
        /// Gets the first endpoint of the edge.
        /// </summary>
        TVertex FirstVertex { get; }

        /// <summary>
        /// Gets the second endpoint of the edge.
        /// </summary>
        TVertex SecondVertex { get; }
    }
}
