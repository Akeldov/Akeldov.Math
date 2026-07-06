namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a weighted edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The weighted edge type.</typeparam>
    /// <typeparam name="TWeight">The edge weight type.</typeparam>
    public interface IWeightedEdge<TVertex, TEdge, TWeight> : IEdge<TVertex, TEdge>
        where TVertex : IWeightedEdgeGraphVertex<TVertex, TEdge, TWeight>
        where TEdge : IWeightedEdge<TVertex, TEdge, TWeight>
    {
        /// <summary>
        /// Gets the edge weight.
        /// </summary>
        TWeight Weight { get; }
    }
}
