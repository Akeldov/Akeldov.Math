namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents three values ordered by chromatic index zero, one, and two.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public readonly struct ChromaticTriplet<T>
    {
        /// <summary>
        /// Initializes a chromatically ordered triplet.
        /// </summary>
        /// <param name="index0">The value associated with chromatic index zero.</param>
        /// <param name="index1">The value associated with chromatic index one.</param>
        /// <param name="index2">The value associated with chromatic index two.</param>
        public ChromaticTriplet(T index0, T index1, T index2)
        {
            Index0 = index0;
            Index1 = index1;
            Index2 = index2;
        }

        /// <summary>
        /// Gets the value associated with chromatic index zero.
        /// </summary>
        public T Index0 { get; }

        /// <summary>
        /// Gets the value associated with chromatic index one.
        /// </summary>
        public T Index1 { get; }

        /// <summary>
        /// Gets the value associated with chromatic index two.
        /// </summary>
        public T Index2 { get; }

        /// <summary>
        /// Deconstructs the triplet in chromatic-index order.
        /// </summary>
        /// <param name="index0">The value associated with chromatic index zero.</param>
        /// <param name="index1">The value associated with chromatic index one.</param>
        /// <param name="index2">The value associated with chromatic index two.</param>
        public void Deconstruct(out T index0, out T index1, out T index2)
        {
            index0 = Index0;
            index1 = Index1;
            index2 = Index2;
        }
    }
}
