namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a PartialTriplet value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct PartialTriplet<T>
    {
        /// <summary>
        /// Performs the PartialTriplet operation.
        /// </summary>
        /// <param name="main">The Main value.</param>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        /// <param name="presence">The Presence value.</param>
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
        /// Performs the PartialTriplet operation.
        /// </summary>
        /// <param name="hasMain">The HasMain value.</param>
        /// <param name="hasLeft">The HasLeft value.</param>
        /// <param name="hasRight">The HasRight value.</param>
        /// <param name="main">The Main value.</param>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
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
        /// Performs the PartialTriplet operation.
        /// </summary>
        /// <param name="presence">The presence value.</param>
        /// <param name="triplet">The Triplet value.</param>
        public PartialTriplet(Triplet<T> triplet, TripletPresenceFlags presence)
            : this(triplet.Main, triplet.Left, triplet.Right, presence)
        {
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
        /// Gets the Presence value.
        /// </summary>
        public TripletPresenceFlags Presence { get; }

        /// <summary>
        /// Performs the HasMain operation.
        /// </summary>
        public bool HasMain => (Presence & TripletPresenceFlags.Main) != 0;

        /// <summary>
        /// Performs the HasLeft operation.
        /// </summary>
        public bool HasLeft => (Presence & TripletPresenceFlags.Left) != 0;

        /// <summary>
        /// Performs the HasRight operation.
        /// </summary>
        public bool HasRight => (Presence & TripletPresenceFlags.Right) != 0;

        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        public Triplet<T> ToTriplet()
        {
            return new Triplet<T>(Main, Left, Right);
        }

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

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="main">The Main value.</param>
        /// <param name="left">The Left value.</param>
        /// <param name="right">The Right value.</param>
        /// <param name="presence">The Presence value.</param>
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
