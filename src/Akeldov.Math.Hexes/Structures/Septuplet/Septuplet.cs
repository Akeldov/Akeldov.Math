namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a main value and six adjacent values ordered by hex-edge index.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct Septuplet<T>
    {
        /// <summary>
        /// Initializes a main value and its six ordered adjacent values.
        /// </summary>
        /// <param name="main">The primary element.</param>
        /// <param name="adjacent0">The element at adjacent position 0.</param>
        /// <param name="adjacent1">The element at adjacent position 1.</param>
        /// <param name="adjacent2">The element at adjacent position 2.</param>
        /// <param name="adjacent3">The element at adjacent position 3.</param>
        /// <param name="adjacent4">The element at adjacent position 4.</param>
        /// <param name="adjacent5">The element at adjacent position 5.</param>
        public Septuplet(
            T main,
            T adjacent0,
            T adjacent1,
            T adjacent2,
            T adjacent3,
            T adjacent4,
            T adjacent5)
        {
            Main = main;
            Adjacent0 = adjacent0;
            Adjacent1 = adjacent1;
            Adjacent2 = adjacent2;
            Adjacent3 = adjacent3;
            Adjacent4 = adjacent4;
            Adjacent5 = adjacent5;
        }

        /// <summary>
        /// Gets the primary element.
        /// </summary>
        public T Main { get; }

        /// <summary>
        /// Gets the element at adjacent position 0.
        /// </summary>
        public T Adjacent0 { get; }

        /// <summary>
        /// Gets the element at adjacent position 1.
        /// </summary>
        public T Adjacent1 { get; }

        /// <summary>
        /// Gets the element at adjacent position 2.
        /// </summary>
        public T Adjacent2 { get; }

        /// <summary>
        /// Gets the element at adjacent position 3.
        /// </summary>
        public T Adjacent3 { get; }

        /// <summary>
        /// Gets the element at adjacent position 4.
        /// </summary>
        public T Adjacent4 { get; }

        /// <summary>
        /// Gets the element at adjacent position 5.
        /// </summary>
        public T Adjacent5 { get; }

        /// <summary>
        /// Deconstructs the values in main-then-adjacency order.
        /// </summary>
        /// <param name="main">Receives the primary element.</param>
        /// <param name="adjacent0">Receives the element at adjacent position 0.</param>
        /// <param name="adjacent1">Receives the element at adjacent position 1.</param>
        /// <param name="adjacent2">Receives the element at adjacent position 2.</param>
        /// <param name="adjacent3">Receives the element at adjacent position 3.</param>
        /// <param name="adjacent4">Receives the element at adjacent position 4.</param>
        /// <param name="adjacent5">Receives the element at adjacent position 5.</param>
        public void Deconstruct(
            out T main,
            out T adjacent0,
            out T adjacent1,
            out T adjacent2,
            out T adjacent3,
            out T adjacent4,
            out T adjacent5)
        {
            main = Main;
            adjacent0 = Adjacent0;
            adjacent1 = Adjacent1;
            adjacent2 = Adjacent2;
            adjacent3 = Adjacent3;
            adjacent4 = Adjacent4;
            adjacent5 = Adjacent5;
        }
    }
}
