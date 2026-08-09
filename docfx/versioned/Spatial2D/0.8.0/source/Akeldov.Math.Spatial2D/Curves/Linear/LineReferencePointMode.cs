namespace Akeldov.Math.Spatial2D.Curves
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
