using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only adjacency contract for a directed graph.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    public interface IDirectedGraph<TVertex> : IGraph<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The target vertex.</param>
        /// <returns>The incoming vertices. The collection is empty when the vertex has no incoming vertices.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TVertex> GetIncomingVertices(TVertex vertex);

        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The source vertex.</param>
        /// <returns>The outgoing vertices. The collection is empty when the vertex has no outgoing vertices.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TVertex> GetOutgoingVertices(TVertex vertex);
    }
}
