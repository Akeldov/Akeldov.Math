namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Specifies a point position relative to the half-planes divided by a directed line.
    /// </summary>
    public enum HalfPlaneSide
    {
        /// <summary>
        /// The point lies on the line.
        /// </summary>
        OnTheLine = 0,

        /// <summary>
        /// The point lies in the left half-plane of the directed line.
        /// </summary>
        Left = 1,

        /// <summary>
        /// The point lies in the right half-plane of the directed line.
        /// </summary>
        Right = -1
    }
}
