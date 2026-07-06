namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph vertex that exposes incident edges.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The incident edge type.</typeparam>
    public interface IEdgeGraphVertex<TVertex, TEdge> : IGraphVertex<TVertex>
        where TVertex : IEdgeGraphVertex<TVertex, TEdge>
        where TEdge : IEdge<TVertex, TEdge>
    {
    }
}
