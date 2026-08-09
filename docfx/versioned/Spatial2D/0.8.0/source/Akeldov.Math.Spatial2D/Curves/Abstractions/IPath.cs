namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a parameterized two-endpoint curve with an explicit traversal direction.
    /// </summary>
    public interface IPath : ITwoEndpointCurve, IParameterizedCurve
    {
        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        PointXY StartPoint { get; }

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        PointXY EndPoint { get; }
    }
}
