namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a Septuplet value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct Septuplet<T>
    {
        /// <summary>
        /// Performs the Septuplet operation.
        /// </summary>
        /// <param name="main">The Main value.</param>
        /// <param name="adjacent0">The Adjacent0 value.</param>
        /// <param name="adjacent1">The Adjacent1 value.</param>
        /// <param name="adjacent2">The Adjacent2 value.</param>
        /// <param name="adjacent3">The Adjacent3 value.</param>
        /// <param name="adjacent4">The Adjacent4 value.</param>
        /// <param name="adjacent5">The Adjacent5 value.</param>
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
        /// Gets the Main value.
        /// </summary>
        public T Main { get; }

        /// <summary>
        /// Gets the Adjacent0 value.
        /// </summary>
        public T Adjacent0 { get; }

        /// <summary>
        /// Gets the Adjacent1 value.
        /// </summary>
        public T Adjacent1 { get; }

        /// <summary>
        /// Gets the Adjacent2 value.
        /// </summary>
        public T Adjacent2 { get; }

        /// <summary>
        /// Gets the Adjacent3 value.
        /// </summary>
        public T Adjacent3 { get; }

        /// <summary>
        /// Gets the Adjacent4 value.
        /// </summary>
        public T Adjacent4 { get; }

        /// <summary>
        /// Gets the Adjacent5 value.
        /// </summary>
        public T Adjacent5 { get; }

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="main">The Main value.</param>
        /// <param name="adjacent0">The Adjacent0 value.</param>
        /// <param name="adjacent1">The Adjacent1 value.</param>
        /// <param name="adjacent2">The Adjacent2 value.</param>
        /// <param name="adjacent3">The Adjacent3 value.</param>
        /// <param name="adjacent4">The Adjacent4 value.</param>
        /// <param name="adjacent5">The Adjacent5 value.</param>
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
