namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a Triplet value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct Triplet<T>
    {
        /// <summary>
        /// Performs the Triplet operation.
        /// </summary>
        /// <param name="main">The main value.</param>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public Triplet(T main, T left, T right)
        {
            Main = main;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the Main value.
        /// </summary>
        public T Main { get; }

        /// <summary>
        /// Gets the Left value.
        /// </summary>
        public T Left { get; }

        /// <summary>
        /// Gets the Right value.
        /// </summary>
        public T Right { get; }

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="main">The main value.</param>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public void Deconstruct(out T main, out T left, out T right)
        {
            main = Main;
            left = Left;
            right = Right;
        }
    }
}
