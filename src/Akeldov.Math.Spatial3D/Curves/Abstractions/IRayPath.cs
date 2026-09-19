#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a parameterized three-dimensional one-endpoint curve with an explicit traversal direction.
    /// </summary>
    /// <remarks>
    /// The curve coordinate increases away from <see cref="Origin"/> along the traversal direction.
    /// </remarks>
    public interface IRayPath : IOneEndpointCurve, IParameterizedCurve
    {
        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        PointXYZ Origin { get; }
    }
}
