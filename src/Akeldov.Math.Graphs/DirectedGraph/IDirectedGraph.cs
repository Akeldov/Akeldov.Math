namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Defines a read-only directed graph.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    public interface IDirectedGraph<TVertex> : IGraph<TVertex>
        where TVertex : IDirectedGraphVertex<TVertex>
    {
    }
}
