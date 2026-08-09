namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Describes how text is positioned relative to its origin.
    /// </summary>
    public enum TextAnchor
    {
        /// <summary>
        /// Positions the left edge of the first line's baseline at the origin.
        /// </summary>
        BaselineLeft,

        /// <summary>
        /// Positions the center of the first line's baseline at the origin.
        /// </summary>
        BaselineCenter,

        /// <summary>
        /// Positions the right edge of the first line's baseline at the origin.
        /// </summary>
        BaselineRight,

        /// <summary>
        /// Positions the top-left visible text bounds at the origin.
        /// </summary>
        TopLeft,

        /// <summary>
        /// Positions the top-center visible text bounds at the origin.
        /// </summary>
        TopCenter,

        /// <summary>
        /// Positions the top-right visible text bounds at the origin.
        /// </summary>
        TopRight,

        /// <summary>
        /// Positions the center-left visible text bounds at the origin.
        /// </summary>
        CenterLeft,

        /// <summary>
        /// Positions the center of the visible text bounds at the origin.
        /// </summary>
        Center,

        /// <summary>
        /// Positions the center-right visible text bounds at the origin.
        /// </summary>
        CenterRight,

        /// <summary>
        /// Positions the bottom-left visible text bounds at the origin.
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Positions the bottom-center visible text bounds at the origin.
        /// </summary>
        BottomCenter,

        /// <summary>
        /// Positions the bottom-right visible text bounds at the origin.
        /// </summary>
        BottomRight
    }
}
