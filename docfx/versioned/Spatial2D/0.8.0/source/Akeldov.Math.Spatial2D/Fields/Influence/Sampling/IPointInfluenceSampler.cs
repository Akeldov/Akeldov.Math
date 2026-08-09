namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Combines point influence sources into a sampled value at a two-dimensional point.
    /// </summary>
    /// <remarks>
    /// This interface narrows <see cref="IInfluenceSampler{TSource, TValue}"/> to point sources.
    /// Implementations may use point centers, distances, and powers when combining source influence.
    /// </remarks>
    /// <typeparam name="TSource">The point influence source type.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public interface IPointInfluenceSampler<TSource, TValue> : IInfluenceSampler<TSource, TValue>
        where TSource : IPointInfluenceSource<TValue>
    {
    }
}
