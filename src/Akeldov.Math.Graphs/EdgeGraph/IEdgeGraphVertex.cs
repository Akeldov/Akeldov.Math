namespace Akeldov.Math.Graphs
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a graph vertex that exposes incident edges.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The incident edge type.</typeparam>
    public interface IEdgeGraphVertex<TVertex, TEdge> : IGraphVertex<TVertex>
        where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of edges incident to this vertex.
        /// </summary>
        IReadOnlyList<TEdge> IncidentEdges { get; }
    }
}
