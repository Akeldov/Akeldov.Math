namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>Represents a parameterized curve that also defines a signed point distance.</summary>
    /// <remarks>
    /// The implementation defines the meaning of the sign; this contract does not imply a filled area.
    /// The magnitude of SignedDistance must agree with the distance returned by ProjectWithParameter.
    /// When the sign depends on the selected projection, both operations must use the same tie-breaking rule.
    /// Curve coordinates are measured in world length units, not normalized to [0, 1].
    /// </remarks>
    public interface ISignedParameterizedCurve : IParameterizedCurve, ISignedPointDistanceProvider
    {
    }
}
