#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Defines how a parameterized line selects the reference point whose projection becomes the curve-coordinate origin.
    /// </summary>
    public enum LineReferencePointMode
    {
        /// <summary>
        /// Uses the global coordinate origin.
        /// </summary>
        GlobalZero,

        /// <summary>
        /// Uses the first point passed to the parameterized line constructor.
        /// </summary>
        PointA,

        /// <summary>
        /// Uses the second point passed to the parameterized line constructor.
        /// </summary>
        PointB,

        /// <summary>
        /// Uses the midpoint of the two points passed to the parameterized line constructor.
        /// </summary>
        Midpoint
    }
}
