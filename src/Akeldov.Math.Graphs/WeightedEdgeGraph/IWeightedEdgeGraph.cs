namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph that exposes weighted edges.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The weighted edge type.</typeparam>
    /// <typeparam name="TWeight">The edge weight type.</typeparam>
    public interface IWeightedEdgeGraph<TVertex, TEdge, TWeight> : IEdgeGraph<TVertex, TEdge>
        where TEdge : IWeightedEdge<TVertex, TWeight>
    {
    }
}
