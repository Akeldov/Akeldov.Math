#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite parameterized three-dimensional two-endpoint curve segment.
    /// </summary>
    public interface IFinitePath : IPath, IFiniteTwoEndpointCurve
    {
    }
}
