using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed graph vertex that exposes incoming and outgoing directed edges.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    public interface IDirectedEdgeGraphVertex<TVertex, TEdge> : IEdgeGraphVertex<TVertex, TEdge>, IDirectedGraphVertex<TVertex>
        where TVertex : IDirectedEdgeGraphVertex<TVertex, TEdge>, IDirectedGraphVertex<TVertex>
        where TEdge : IDirectedEdge<TVertex, TEdge>
    {
        /// <summary>
        /// Gets the read-only structural collection of edges directed into this vertex.
        /// </summary>
        IReadOnlyList<TEdge> IncomingEdges { get; }

        /// <summary>
        /// Gets the read-only structural collection of edges directed out of this vertex.
        /// </summary>
        IReadOnlyList<TEdge> OutgoingEdges { get; }
    }
}
