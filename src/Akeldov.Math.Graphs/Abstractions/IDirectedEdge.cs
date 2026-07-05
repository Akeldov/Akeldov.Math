namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a directed edge between two graph vertices.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    /// <remarks>
    /// <see cref="FromVertex"/> represents the source vertex and <see cref="ToVertex"/>
    /// represents the target vertex. Implementations should expose the same vertices through
    /// <see cref="IEdge{TVertex}.FirstVertex"/> and <see cref="IEdge{TVertex}.SecondVertex"/>.
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
