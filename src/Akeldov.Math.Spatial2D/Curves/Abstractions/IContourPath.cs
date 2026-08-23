namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed path that provides the spatial queries required to form a contour.
    /// </summary>
    public interface IContourPath : IFinitePath, IRightwardCrossingProvider, IRayIntersectionProvider
    {
    }
}
