namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents a spatial field that can be sampled at a two-dimensional point.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the field.</typeparam>
    public interface IField<TValue>
    {
        /// <summary>
        /// Samples the field value at the specified point.
        /// </summary>
        /// <param name="point">The point to sample.</param>
        /// <returns>The field value at the specified point.</returns>
        TValue Sample(PointXY point);
    }
}
