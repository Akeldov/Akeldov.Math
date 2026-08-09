namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Specifies a named boundary point where rectangular contour curve coordinate zero lies.
    /// </summary>
    public enum RectangleContourParameterOrigin
    {
        /// <summary>
        /// The midpoint of the right edge.
        /// </summary>
        RightEdgeMidpoint = 0,

        /// <summary>
        /// The top-right corner.
        /// </summary>
        TopRight = 1,

        /// <summary>
        /// The midpoint of the top edge.
        /// </summary>
        TopEdgeMidpoint = 2,

        /// <summary>
        /// The top-left corner.
        /// </summary>
        TopLeft = 3,

        /// <summary>
        /// The midpoint of the left edge.
        /// </summary>
        LeftEdgeMidpoint = 4,

        /// <summary>
        /// The bottom-left corner.
        /// </summary>
        BottomLeft = 5,

        /// <summary>
        /// The midpoint of the bottom edge.
        /// </summary>
        BottomEdgeMidpoint = 6,

        /// <summary>
        /// The bottom-right corner.
        /// </summary>
        BottomRight = 7
    }
}
