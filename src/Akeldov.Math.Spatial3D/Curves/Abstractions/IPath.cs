#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a parameterized three-dimensional two-endpoint curve with an explicit traversal direction.
    /// </summary>
    public interface IPath : ITwoEndpointCurve, IParameterizedCurve
    {
        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        PointXYZ StartPoint { get; }

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        PointXYZ EndPoint { get; }
    }
}
