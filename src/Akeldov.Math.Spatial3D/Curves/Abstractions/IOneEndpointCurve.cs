#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a three-dimensional curve with a single endpoint.
    /// </summary>
    public interface IOneEndpointCurve : ICurve
    {
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        PointXYZ Endpoint { get; }
    }
}
