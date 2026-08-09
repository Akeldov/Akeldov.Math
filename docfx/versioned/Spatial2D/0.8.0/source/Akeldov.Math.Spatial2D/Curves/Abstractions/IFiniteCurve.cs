namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a curve with finite length.
    /// </summary>
    public interface IFiniteCurve : ICurve
    {
        /// <summary>
        /// Gets the finite non-negative curve length in world coordinate units.
        /// </summary>
        float Length { get; }
    }
}
