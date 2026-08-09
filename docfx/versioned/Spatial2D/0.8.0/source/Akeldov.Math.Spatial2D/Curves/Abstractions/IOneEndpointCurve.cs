namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a curve with a single endpoint.
    /// </summary>
    public interface IOneEndpointCurve : ICurve
    {
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        PointXY Endpoint { get; }
    }
}
