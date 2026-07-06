using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph that exposes its edge collection.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    public interface IEdgeGraph<TVertex, TEdge> : IGraph<TVertex>
        where TVertex : IEdgeGraphVertex<TVertex, TEdge>
        where TEdge : IEdge<TVertex, TEdge>
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
        IReadOnlyCollection<TEdge> GetIncidentEdges(TVertex vertex);
    }
}
