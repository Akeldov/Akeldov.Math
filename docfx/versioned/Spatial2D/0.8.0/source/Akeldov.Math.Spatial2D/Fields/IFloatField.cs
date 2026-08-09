namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a spatial field that produces floating-point values.
    /// </summary>
    /// <remarks>
    /// Implementations must return values in the inclusive range from <see cref="Min"/> to <see cref="Max"/>.
    /// </remarks>
    public interface IFloatField : IField<float>
    {
        /// <summary>
        /// Gets the minimum value that can be returned by <see cref="IField{TValue}.Sample"/>.
        /// </summary>
        float Min { get; }

        /// <summary>
        /// Gets the maximum value that can be returned by <see cref="IField{TValue}.Sample"/>.
        /// </summary>
        float Max { get; }
    }
}
