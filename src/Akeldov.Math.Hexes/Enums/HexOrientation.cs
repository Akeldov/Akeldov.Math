namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Specifies whether hexagons present a vertex or an edge at the top.
    /// </summary>
    public enum HexOrientation
    {
        /// <summary>
        /// Hexagons have a vertex at the top and use row-oriented offset layouts.
        /// </summary>
        PointyTop,
        /// <summary>
        /// Hexagons have a horizontal edge at the top and use column-oriented offset layouts.
        /// </summary>
        FlatTop
    }
}
