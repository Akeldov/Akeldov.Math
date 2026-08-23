using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents an influence source backed by a parameterized projectable curve.
    /// </summary>
    /// <remarks>
    /// Curve influence sources can measure distance and project sampled points onto the underlying curve.
    /// </remarks>
    public interface ICurveInfluenceSource : IInfluenceSource, IParameterizedCurve
    {
    }

    /// <summary>
    /// Represents a typed influence source backed by a parameterized projectable curve.
    /// </summary>
    /// <typeparam name="TValue">The value type contributed by the curve.</typeparam>
    public interface ICurveInfluenceSource<TValue> : ICurveInfluenceSource, IInfluenceSource<TValue>
    {
    }
}
