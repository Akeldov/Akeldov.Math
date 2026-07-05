namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a spatial field that produces integer values.
    /// </summary>
    /// <remarks>
    /// Implementations must return values in the inclusive range from <see cref="Min"/> to <see cref="Max"/>.
    /// </remarks>
    public interface IIntField : IField<int>
    {
        /// <summary>
        /// Gets the minimum value that can be returned by <see cref="IField{TValue}.Sample"/>.
        /// </summary>
        int Min { get; }

        /// <summary>
        /// Gets the maximum value that can be returned by <see cref="IField{TValue}.Sample"/>.
        /// </summary>
        int Max { get; }
    }
}
