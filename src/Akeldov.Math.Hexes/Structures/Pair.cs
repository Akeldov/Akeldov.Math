namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a Pair value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct Pair<T>
    {
        /// <summary>
        /// Performs the Pair operation.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public Pair(T left, T right)
        {
            Left = left;
            Right = right;
        }

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
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public void Deconstruct(out T left, out T right)
        {
            left = Left;
            right = Right;
        }
    }
}
