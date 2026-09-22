#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite directed closed path in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// <see cref="IPath.StartPoint"/> and <see cref="IPath.EndPoint"/> represent the same geometric point.
    /// A closed path need not be planar and does not by itself define an enclosed volume.
    /// </remarks>
    public interface IClosedPath : IFinitePath
    {
    }
}
