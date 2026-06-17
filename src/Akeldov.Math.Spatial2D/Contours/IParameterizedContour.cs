using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour with a length-based curve coordinate.
    /// </summary>
    public interface IParameterizedContour : IContour, IParameterizedCurve, IFinitePath
    {
    }
}
