namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Describes a polyhex with an integer priority used to order competing shapes.
    /// </summary>
    public interface IPolyhexWithPriority : IPolyhex
    {
        /// <summary>
        /// Gets the ordering priority of the polyhex.
        /// </summary>
        int Priority { get; }
    }
}
