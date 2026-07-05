namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Combines curve influence sources into a sampled value at a two-dimensional point.
    /// </summary>
    /// <remarks>
    /// This interface narrows <see cref="IInfluenceSampler{TSource, TValue}"/> to projectable
    /// curve sources. Implementations may use curve projections, distances, and curve parameters
    /// when combining source influence.
    /// </remarks>
    /// <typeparam name="TCurveSource">The curve influence source type.</typeparam>
    /// <typeparam name="TValue">The sampled value type.</typeparam>
    public interface ICurveInfluenceSampler<TCurveSource, TValue> : IInfluenceSampler<TCurveSource, TValue>
        where TCurveSource : ICurveInfluenceSource<TValue>
    {
    }
}
