using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed graph that exposes its edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    public interface IDirectedEdgeGraph<TVertex, TEdge> : IDirectedGraph<TVertex>, IEdgeGraph<TVertex, TEdge>
        where TEdge : IDirectedEdge<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The target vertex.</param>
        /// <returns>The incoming edges. The collection is empty when the vertex has no incoming edges.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TEdge> GetIncomingEdges(TVertex vertex);

        /// <summary>
        /// Gets the read-only structural collection of edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The source vertex.</param>
        /// <returns>The outgoing edges. The collection is empty when the vertex has no outgoing edges.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TEdge> GetOutgoingEdges(TVertex vertex);
    }
}
