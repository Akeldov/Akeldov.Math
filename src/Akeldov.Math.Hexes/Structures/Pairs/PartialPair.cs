namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents an ordered pair whose positions carry explicit presence information.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct PartialPair<T>
    {
        /// <summary>
        /// Initializes a partial pair from stored values and presence flags.
        /// </summary>
        /// <param name="left">The value stored at the left position.</param>
        /// <param name="right">The value stored at the right position.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialPair(
            T left,
            T right,
            PairPresenceFlags presence)
        {
            Left = left;
            Right = right;
            Presence = presence;
        }

        /// <summary>
        /// Initializes a partial pair from stored values and per-position presence indicators.
        /// </summary>
        /// <param name="left">The value stored at the left position.</param>
        /// <param name="right">The value stored at the right position.</param>
        /// <param name="hasLeft">Whether the left position is present.</param>
        /// <param name="hasRight">Whether the right position is present.</param>
        public PartialPair(
            T left,
            T right,
            bool hasLeft,
            bool hasRight)
            : this(left, right, CreatePresence(hasLeft, hasRight))
        {
        }

        /// <summary>
        /// Initializes a partial pair from a complete pair and presence flags.
        /// </summary>
        /// <param name="pair">The complete pair whose values are stored.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialPair(Pair<T> pair, PairPresenceFlags presence)
            : this(pair.Left, pair.Right, presence)
        {
        }

        /// <summary>
        /// Gets the value stored at the left position, regardless of presence.
        /// </summary>
        public T Left { get; }

        /// <summary>
        /// Gets the value stored at the right position, regardless of presence.
        /// </summary>
        public T Right { get; }

        /// <summary>
        /// Gets the flags identifying the semantically present positions.
        /// </summary>
        public PairPresenceFlags Presence { get; }

        /// <summary>
        /// Gets whether the left position is present.
        /// </summary>
        public bool HasLeft => (Presence & PairPresenceFlags.Left) != 0;

        /// <summary>
        /// Gets whether the right position is present.
        /// </summary>
        public bool HasRight => (Presence & PairPresenceFlags.Right) != 0;

        /// <summary>
        /// Returns the stored values as a complete pair, discarding presence information.
        /// </summary>
        public Pair<T> ToPair()
        {
            return new Pair<T>(Left, Right);
        }

        /// <summary>
        /// Deconstructs the stored values in left-to-right order.
        /// </summary>
        /// <param name="left">Receives the stored left value.</param>
        /// <param name="right">Receives the stored right value.</param>
        public void Deconstruct(out T left, out T right)
        {
            left = Left;
            right = Right;
        }

        /// <summary>
        /// Deconstructs the stored values and their presence flags.
        /// </summary>
        /// <param name="left">Receives the stored left value.</param>
        /// <param name="right">Receives the stored right value.</param>
        /// <param name="presence">Receives the present-position flags.</param>
        public void Deconstruct(
            out T left,
            out T right,
            out PairPresenceFlags presence)
        {
            left = Left;
            right = Right;
            presence = Presence;
        }

        private static PairPresenceFlags CreatePresence(bool hasLeft, bool hasRight)
        {
            PairPresenceFlags presence = PairPresenceFlags.None;

            if (hasLeft)
                presence |= PairPresenceFlags.Left;

            if (hasRight)
                presence |= PairPresenceFlags.Right;

            return presence;
        }
    }
}
