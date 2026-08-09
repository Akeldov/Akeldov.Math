namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents an object that can be scaled by a two-dimensional factor.
    /// </summary>
    /// <typeparam name="TSelf">The scaled result type.</typeparam>
    public interface IScalable<out TSelf>
    {
        /// <summary>
        /// Returns this object scaled by the specified factor.
        /// </summary>
        /// <param name="scale">The scale factor.</param>
        /// <returns>The scaled object.</returns>
        TSelf Scale(VectorXY scale);
    }
}
