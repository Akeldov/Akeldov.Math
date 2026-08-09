namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents an object that can influence sampled field values and measure distance to a point.
    /// </summary>
    public interface IInfluenceSource : IPointDistanceProvider
    {
    }

    /// <summary>
    /// Represents an influence source that contributes typed values to a field.
    /// </summary>
    /// <typeparam name="TValue">The value type contributed by the source.</typeparam>
    public interface IInfluenceSource<TValue> : IInfluenceSource
    {
        /// <summary>
        /// Gets the influence contribution of this source for the specified point.
        /// </summary>
        /// <param name="point">The finite point being sampled.</param>
        /// <returns>
        /// The value, closest/source point, distance, and weight used by influence samplers.
        /// </returns>
        InfluenceSample<TValue> GetInfluence(PointXY point);
    }
}
