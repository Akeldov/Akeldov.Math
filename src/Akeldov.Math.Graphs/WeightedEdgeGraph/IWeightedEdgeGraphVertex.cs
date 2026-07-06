namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph vertex that exposes weighted incident edges.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The weighted edge type.</typeparam>
    /// <typeparam name="TWeight">The edge weight type.</typeparam>
    public interface IWeightedEdgeGraphVertex<TVertex, TEdge, TWeight> : IEdgeGraphVertex<TVertex, TEdge>
        where TEdge : IWeightedEdge<TVertex, TWeight>
    {
    }
}
