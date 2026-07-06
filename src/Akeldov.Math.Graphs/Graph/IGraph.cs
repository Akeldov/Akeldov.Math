using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only graph over vertices that expose adjacency information.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    public interface IGraph<TVertex>
        where TVertex : IGraphVertex<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices in the graph.
        /// </summary>
        IReadOnlyCollection<TVertex> Vertices { get; }
    }
}
