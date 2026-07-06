namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a weighted edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TWeight">The edge weight type.</typeparam>
    public interface IWeightedEdge<TVertex, TWeight> : IEdge<TVertex>
    {
        /// <summary>
        /// Gets the edge weight.
        /// </summary>
        TWeight Weight { get; }
    }
}
