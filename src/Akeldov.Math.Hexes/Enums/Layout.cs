namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Specifies the offset-coordinate layout and hex orientation of a rectangular hex grid.
    /// </summary>
    public enum Layout
    {
        /// <summary>
        /// Pointy-top layout whose odd-numbered rows are shifted right.
        /// </summary>
        OddR = 0,
        /// <summary>
        /// Pointy-top layout whose even-numbered rows are shifted right.
        /// </summary>
        EvenR = 1,
        /// <summary>
        /// Flat-top layout whose odd-numbered columns are shifted down.
        /// </summary>
        OddQ = 2,
        /// <summary>
        /// Flat-top layout whose even-numbered columns are shifted down.
        /// </summary>
        EvenQ = 3
    }
}
