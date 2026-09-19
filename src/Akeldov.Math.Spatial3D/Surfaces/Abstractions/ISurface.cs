#pragma warning disable IDE0130 // Subfolders organize sources within the public Surfaces namespace.
namespace Akeldov.Math.Spatial3D.Surfaces
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a surface in three-dimensional space that provides unsigned distances to points.
    /// </summary>
    public interface ISurface : IPointDistanceProvider
    {
    }
}
