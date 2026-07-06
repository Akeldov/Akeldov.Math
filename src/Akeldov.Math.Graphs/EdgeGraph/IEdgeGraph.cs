using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph that exposes its edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    public interface IEdgeGraph<TVertex, TEdge> : IGraph<TVertex>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of edges in the graph.
        /// </summary>
        IReadOnlyCollection<TEdge> Edges { get; }

        /// <summary>
        /// Gets the read-only structural collection of edges incident to the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incident edges should be returned.</param>
        /// <returns>The edges incident to <paramref name="vertex"/>.</returns>
        IReadOnlyList<TEdge> GetIncidentEdges(TVertex vertex);
    }
}
