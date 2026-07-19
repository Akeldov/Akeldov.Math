namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a main value and its ordered left and right values.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct Triplet<T>
    {
        /// <summary>
        /// Initializes an ordered main-left-right triplet.
        /// </summary>
        /// <param name="main">The primary element.</param>
        /// <param name="left">The element to the left of the primary element.</param>
        /// <param name="right">The element to the right of the primary element.</param>
        public Triplet(T main, T left, T right)
        {
            Main = main;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the primary element.
        /// </summary>
        public T Main { get; }

        /// <summary>
        /// Gets the left element.
        /// </summary>
        public T Left { get; }

        /// <summary>
        /// Gets the right element.
        /// </summary>
        public T Right { get; }

        /// <summary>
        /// Deconstructs the triplet in main-left-right order.
        /// </summary>
        /// <param name="main">Receives the primary element.</param>
        /// <param name="left">Receives the left element.</param>
        /// <param name="right">Receives the right element.</param>
        public void Deconstruct(out T main, out T left, out T right)
        {
            main = Main;
            left = Left;
            right = Right;
        }
    }
}
