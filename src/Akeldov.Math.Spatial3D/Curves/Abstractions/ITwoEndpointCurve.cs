#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a three-dimensional curve with two unordered endpoints.
    /// </summary>
    /// <remarks>
    /// <see cref="EndpointA"/> and <see cref="EndpointB"/> identify the two boundary points of the curve.
    /// They do not imply traversal direction. Use <see cref="IPath"/> when ordered
    /// start and end points are part of the contract.
    /// </remarks>
    public interface ITwoEndpointCurve : ICurve
    {
        /// <summary>
        /// Gets one endpoint.
        /// </summary>
        PointXYZ EndpointA { get; }

        /// <summary>
        /// Gets the other endpoint.
        /// </summary>
        PointXYZ EndpointB { get; }
    }
}
