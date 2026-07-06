using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed graph that exposes its directed edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    /// <remarks>
    /// For directed edge graphs, <see cref="IEdgeGraph{TVertex, TEdge}.GetIncidentEdges(TVertex)"/>
    /// returns edges touching the specified vertex, including incoming and outgoing edges.
    /// </remarks>
    public interface IDirectedEdgeGraph<TVertex, TEdge> : IDirectedGraph<TVertex>, IEdgeGraph<TVertex, TEdge>
        where TEdge : IDirectedEdge<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incoming edges should be returned.</param>
        /// <returns>The edges directed into <paramref name="vertex"/>.</returns>
        IReadOnlyList<TEdge> GetIncomingEdges(TVertex vertex);

        /// <summary>
        /// Gets the read-only structural collection of edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose outgoing edges should be returned.</param>
        /// <returns>The edges directed out of <paramref name="vertex"/>.</returns>
        IReadOnlyList<TEdge> GetOutgoingEdges(TVertex vertex);
    }
}
