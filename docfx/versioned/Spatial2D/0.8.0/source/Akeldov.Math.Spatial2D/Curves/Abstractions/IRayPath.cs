namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a parameterized one-endpoint curve with an explicit traversal direction.
    /// </summary>
    public interface IRayPath : IOneEndpointCurve, IParameterizedCurve
    {
        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        PointXY Origin { get; }
    }
}
