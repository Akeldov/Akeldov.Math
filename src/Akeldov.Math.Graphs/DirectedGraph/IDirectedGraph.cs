using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only directed graph.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <remarks>
    /// For directed graphs, <see cref="IGraph{TVertex}.GetAdjacentVertices(TVertex)"/> is
    /// equivalent to <see cref="GetOutgoingVertices"/>.
    /// </remarks>
    public interface IDirectedGraph<TVertex> : IGraph<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incoming vertices should be returned.</param>
        /// <returns>The vertices with edges directed into <paramref name="vertex"/>.</returns>
        IReadOnlyList<TVertex> GetIncomingVertices(TVertex vertex);

        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose outgoing vertices should be returned.</param>
        /// <returns>The vertices with edges directed out of <paramref name="vertex"/>.</returns>
        IReadOnlyList<TVertex> GetOutgoingVertices(TVertex vertex);
    }
}
