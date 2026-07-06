namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed graph that exposes its directed edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    public interface IDirectedEdgeGraph<TVertex, TEdge> : IDirectedGraph<TVertex>, IEdgeGraph<TVertex, TEdge>
        where TVertex : IDirectedEdgeGraphVertex<TVertex, TEdge>
        where TEdge : IDirectedEdge<TVertex, TEdge>
    {
    }
}
