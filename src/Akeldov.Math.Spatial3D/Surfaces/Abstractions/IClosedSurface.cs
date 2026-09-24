#pragma warning disable IDE0130 // Subfolders organize sources within the public Surfaces namespace.
namespace Akeldov.Math.Spatial3D.Surfaces
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a bounded, closed surface without boundary edges in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// Typical examples include spheres and tori.
    /// The surface unambiguously separates an enclosed interior from the exterior.
    /// Inherited distance and projection operations apply to the surface itself.
    /// <see cref="ISignedPointDistanceProvider.SignedDistance"/> is negative inside,
    /// zero on the surface, and positive outside.
    /// </remarks>
    public interface IClosedSurface : ISurface, ISignedPointDistanceProvider
    {
        /// <summary>
        /// Determines whether the specified point lies inside or on this closed surface.
        /// </summary>
        /// <param name="point">The finite point to test.</param>
        /// <returns><see langword="true"/> if the point lies inside or on the closed surface; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has a non-finite coordinate.
        /// </exception>
        bool Encloses(PointXYZ point);
    }
}
