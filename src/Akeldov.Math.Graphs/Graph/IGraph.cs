using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only graph.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    public interface IGraph<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices in the graph.
        /// </summary>
        IReadOnlyCollection<TVertex> Vertices { get; }

        /// <summary>
        /// Gets the read-only structural collection of vertices adjacent to the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose adjacent vertices should be returned.</param>
        /// <returns>The vertices adjacent to <paramref name="vertex"/>.</returns>
        IReadOnlyList<TVertex> GetAdjacentVertices(TVertex vertex);
    }
}
