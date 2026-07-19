namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents an ordered pair of values.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct Pair<T>
    {
        /// <summary>
        /// Initializes an ordered pair.
        /// </summary>
        /// <param name="left">The left element.</param>
        /// <param name="right">The right element.</param>
        public Pair(T left, T right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the left element.
        /// </summary>
        public T Left { get; }

        /// <summary>
        /// Gets the right element.
        /// </summary>
        public T Right { get; }

        /// <summary>
        /// Deconstructs the pair in left-to-right order.
        /// </summary>
        /// <param name="left">Receives the left element.</param>
        /// <param name="right">Receives the right element.</param>
        public void Deconstruct(out T left, out T right)
        {
            left = Left;
            right = Right;
        }
    }
}
