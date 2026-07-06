namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    public interface IDirectedEdge<TVertex, TEdge> : IEdge<TVertex, TEdge>
        where TVertex : IDirectedEdgeGraphVertex<TVertex, TEdge>, IDirectedGraphVertex<TVertex>
        where TEdge : IDirectedEdge<TVertex, TEdge>
    {
        /// <summary>
        /// Gets the source vertex of the directed edge.
        /// </summary>
        TVertex FromVertex { get; }

        /// <summary>
        /// Gets the target vertex of the directed edge.
        /// </summary>
        TVertex ToVertex { get; }
    }
}
