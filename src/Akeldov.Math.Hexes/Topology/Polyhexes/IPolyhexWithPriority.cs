namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Defines a contract for IPolyhexWithPriority implementations.
    /// </summary>
    public interface IPolyhexWithPriority : IPolyhex
    {
        /// <summary>
        /// Represents the <c>Priority</c> value.
        /// </summary>
        int Priority { get; }
    }
}
