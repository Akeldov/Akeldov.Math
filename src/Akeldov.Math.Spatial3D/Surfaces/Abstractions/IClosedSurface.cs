#pragma warning disable IDE0130 // Subfolders organize sources within the public Surfaces namespace.
namespace Akeldov.Math.Spatial3D.Surfaces
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a bounded, closed surface without boundary edges in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// Typical examples include spheres and tori.
    /// Inherited distance and projection operations apply to the surface itself.
    /// </remarks>
    public interface IClosedSurface : ISurface
    {
    }
}
