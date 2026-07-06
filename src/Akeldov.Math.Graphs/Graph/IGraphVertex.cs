using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a graph vertex that exposes adjacent vertices directly.
    /// </summary>
    /// <typeparam name="TVertex">The adjacent vertex type.</typeparam>
    public interface IGraphVertex<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of adjacent vertices.
        /// </summary>
        IReadOnlyList<TVertex> Adjacents { get; }
    }
}
