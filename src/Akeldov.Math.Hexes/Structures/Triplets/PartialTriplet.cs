namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a main-left-right triplet whose positions carry explicit presence information.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct PartialTriplet<T>
    {
        /// <summary>
        /// Initializes a partial triplet from stored values and presence flags.
        /// </summary>
        /// <param name="main">The value stored at the primary position.</param>
        /// <param name="left">The value stored at the left position.</param>
        /// <param name="right">The value stored at the right position.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialTriplet(
            T main,
            T left,
            T right,
            TripletPresenceFlags presence)
        {
            Main = main;
            Left = left;
            Right = right;
            Presence = presence;
        }

        /// <summary>
        /// Initializes a partial triplet from stored values and per-position presence indicators.
        /// </summary>
        /// <param name="main">The value stored at the primary position.</param>
        /// <param name="left">The value stored at the left position.</param>
        /// <param name="right">The value stored at the right position.</param>
        /// <param name="hasMain">Whether the primary position is present.</param>
        /// <param name="hasLeft">Whether the left position is present.</param>
        /// <param name="hasRight">Whether the right position is present.</param>
        public PartialTriplet(
            T main,
            T left,
            T right,
            bool hasMain,
            bool hasLeft,
            bool hasRight)
            : this(main, left, right, CreatePresence(hasMain, hasLeft, hasRight))
        {
        }

        /// <summary>
        /// Initializes a partial triplet from a complete triplet and presence flags.
        /// </summary>
        /// <param name="triplet">The complete triplet whose values are stored.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialTriplet(Triplet<T> triplet, TripletPresenceFlags presence)
            : this(triplet.Main, triplet.Left, triplet.Right, presence)
        {
        }

        /// <summary>
        /// Gets the value stored at the primary position, regardless of presence.
        /// </summary>
        public T Main { get; }

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
        public TripletPresenceFlags Presence { get; }

        /// <summary>
        /// Gets whether the primary position is present.
        /// </summary>
        public bool HasMain => (Presence & TripletPresenceFlags.Main) != 0;

        /// <summary>
        /// Gets whether the left position is present.
        /// </summary>
        public bool HasLeft => (Presence & TripletPresenceFlags.Left) != 0;

        /// <summary>
        /// Gets whether the right position is present.
        /// </summary>
        public bool HasRight => (Presence & TripletPresenceFlags.Right) != 0;

        /// <summary>
        /// Returns the stored values as a complete triplet, discarding presence information.
        /// </summary>
        public Triplet<T> ToTriplet()
        {
            return new Triplet<T>(Main, Left, Right);
        }

        /// <summary>
        /// Deconstructs the stored values in main-left-right order.
        /// </summary>
        /// <param name="main">Receives the stored primary value.</param>
        /// <param name="left">Receives the stored left value.</param>
        /// <param name="right">Receives the stored right value.</param>
        public void Deconstruct(out T main, out T left, out T right)
        {
            main = Main;
            left = Left;
            right = Right;
        }

        /// <summary>
        /// Deconstructs the stored values and their presence flags.
        /// </summary>
        /// <param name="main">Receives the stored primary value.</param>
        /// <param name="left">Receives the stored left value.</param>
        /// <param name="right">Receives the stored right value.</param>
        /// <param name="presence">Receives the present-position flags.</param>
        public void Deconstruct(
            out T main,
            out T left,
            out T right,
            out TripletPresenceFlags presence)
        {
            main = Main;
            left = Left;
            right = Right;
            presence = Presence;
        }

        private static TripletPresenceFlags CreatePresence(bool hasMain, bool hasLeft, bool hasRight)
        {
            TripletPresenceFlags presence = TripletPresenceFlags.None;

            if (hasMain)
                presence |= TripletPresenceFlags.Main;

            if (hasLeft)
                presence |= TripletPresenceFlags.Left;

            if (hasRight)
                presence |= TripletPresenceFlags.Right;

            return presence;
        }
    }
}
