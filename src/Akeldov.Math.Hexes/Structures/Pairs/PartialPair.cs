namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a PartialPair value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct PartialPair<T>
    {
        /// <summary>
        /// Performs the PartialPair operation.
        /// </summary>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        /// <param name="presence">The Presence value.</param>
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
        /// Performs the PartialPair operation.
        /// </summary>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        /// <param name="hasLeft">The HasLeft value.</param>
        /// <param name="hasRight">The HasRight value.</param>
        public PartialPair(
            T left,
            T right,
            bool hasLeft,
            bool hasRight)
            : this(left, right, CreatePresence(hasLeft, hasRight))
        {
        }

        /// <summary>
        /// Performs the PartialPair operation.
        /// </summary>
        /// <param name="pair">The Pair value.</param>
        /// <param name="presence">The presence value.</param>
        public PartialPair(Pair<T> pair, PairPresenceFlags presence)
            : this(pair.Left, pair.Right, presence)
        {
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
        /// Gets the Presence value.
        /// </summary>
        public PairPresenceFlags Presence { get; }

        /// <summary>
        /// Performs the HasLeft operation.
        /// </summary>
        public bool HasLeft => (Presence & PairPresenceFlags.Left) != 0;

        /// <summary>
        /// Performs the HasRight operation.
        /// </summary>
        public bool HasRight => (Presence & PairPresenceFlags.Right) != 0;

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        public Pair<T> ToPair()
        {
            return new Pair<T>(Left, Right);
        }

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        public void Deconstruct(out T left, out T right)
        {
            left = Left;
            right = Right;
        }

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        /// <param name="presence">The Presence value.</param>
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

