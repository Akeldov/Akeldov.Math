namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <remarks>
    /// For directed edges, <see cref="IEdge{TVertex}.FirstVertex"/> is equivalent to
    /// <see cref="FromVertex"/>, and <see cref="IEdge{TVertex}.SecondVertex"/> is equivalent to
    /// <see cref="ToVertex"/>.
    /// </remarks>
    public interface IDirectedEdge<TVertex> : IEdge<TVertex>
    {
        /// <summary>
        /// Gets the source vertex of the directed edge.
        /// </summary>
        TVertex FromVertex { get; }

        /// <summary>
        /// Gets the target vertex of the directed edge.
        /// </summary>
        TVertex ToVertex { get; }
    }
}
