#pragma warning disable IDE0130 // Subfolders organize sources within the public Surfaces namespace.
namespace Akeldov.Math.Spatial3D.Surfaces
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a surface in three-dimensional space that can measure distances to points and project points onto itself.
    /// </summary>
    public interface ISurface : IPointDistanceProvider
    {
        /// <summary>
        /// Projects the specified point onto this surface.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The projection point and distance to this surface.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has a non-finite coordinate.
        /// </exception>
        SurfaceProjection Project(PointXYZ point);
    }
}
