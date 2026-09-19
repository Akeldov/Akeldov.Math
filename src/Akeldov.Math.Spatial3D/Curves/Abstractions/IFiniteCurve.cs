#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a three-dimensional curve with finite length.
    /// </summary>
    public interface IFiniteCurve : ICurve
    {
        /// <summary>
        /// Gets the finite non-negative curve length in world coordinate units.
        /// </summary>
        float Length { get; }
    }
}
