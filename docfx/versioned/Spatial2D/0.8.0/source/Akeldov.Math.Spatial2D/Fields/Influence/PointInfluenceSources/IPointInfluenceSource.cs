namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents an influence source located at a point.
    /// </summary>
    public interface IPointInfluenceSource : IInfluenceSource, IHasPosition2D
    {
        /// <summary>
        /// Gets the source weight used by influence samplers.
        /// </summary>
        float Weight { get; }
    }

    /// <summary>
    /// Represents a point influence source that contributes typed values.
    /// </summary>
    /// <typeparam name="TValue">The value type contributed by the source.</typeparam>
    public interface IPointInfluenceSource<TValue> : IPointInfluenceSource, IInfluenceSource<TValue>
    {
        /// <summary>
        /// Gets the value contributed by this source.
        /// </summary>
        TValue Value { get; }
    }
}
