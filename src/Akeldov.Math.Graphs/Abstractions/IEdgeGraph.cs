using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph that exposes its edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
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
        /// <param name="vertex">The vertex whose incident edges are requested.</param>
        /// <returns>The incident edges. The collection is empty when the vertex has no incident edges.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TEdge> GetIncidentEdges(TVertex vertex);
    }
}
