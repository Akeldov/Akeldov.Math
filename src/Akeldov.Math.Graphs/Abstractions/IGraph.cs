using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only adjacency contract for a graph.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    /// <remarks>
    /// The graph may be directed or undirected. For directed implementations,
    /// <see cref="GetAdjacentVertices"/> represents outgoing adjacency unless a more
    /// specific interface defines otherwise.
    /// </remarks>
    public interface IGraph<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices in the graph.
        /// </summary>
        IReadOnlyCollection<TVertex> Vertices { get; }

        /// <summary>
        /// Determines whether the graph contains the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to locate.</param>
        /// <returns><see langword="true"/> if the vertex belongs to the graph; otherwise, <see langword="false"/>.</returns>
        bool ContainsVertex(TVertex vertex);

        /// <summary>
        /// Gets the read-only structural collection of vertices adjacent to the specified vertex.
        /// </summary>
        /// <param name="vertex">The source vertex.</param>
        /// <returns>The adjacent vertices. The collection is empty when the vertex has no adjacent vertices.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when <paramref name="vertex"/> does not belong to the graph.</exception>
        IReadOnlyCollection<TVertex> GetAdjacentVertices(TVertex vertex);
    }
}
