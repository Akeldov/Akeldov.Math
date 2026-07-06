using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed graph vertex that exposes incoming and outgoing adjacent vertices.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <remarks>
    /// For directed vertices, <see cref="IGraphVertex{TVertex}.Adjacents"/> is equivalent to
    /// <see cref="OutgoingVertices"/>.
    /// </remarks>
    public interface IDirectedGraphVertex<TVertex> : IGraphVertex<TVertex>
        where TVertex : IDirectedGraphVertex<TVertex>
    {
        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed into this vertex.
        /// </summary>
        IReadOnlyList<TVertex> IncomingVertices { get; }

        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed out of this vertex.
        /// </summary>
        IReadOnlyList<TVertex> OutgoingVertices { get; }
    }
}
