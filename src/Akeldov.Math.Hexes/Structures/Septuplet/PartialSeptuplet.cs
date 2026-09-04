namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a main value and six adjacent values with explicit per-position presence information.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public readonly struct PartialSeptuplet<T>
    {
        /// <summary>
        /// Initializes a partial septuplet from stored values and presence flags.
        /// </summary>
        /// <param name="main">The value stored at the primary position.</param>
        /// <param name="adjacent0">The value stored at adjacent position 0.</param>
        /// <param name="adjacent1">The value stored at adjacent position 1.</param>
        /// <param name="adjacent2">The value stored at adjacent position 2.</param>
        /// <param name="adjacent3">The value stored at adjacent position 3.</param>
        /// <param name="adjacent4">The value stored at adjacent position 4.</param>
        /// <param name="adjacent5">The value stored at adjacent position 5.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialSeptuplet(
            T main,
            T adjacent0,
            T adjacent1,
            T adjacent2,
            T adjacent3,
            T adjacent4,
            T adjacent5,
            SeptupletPresenceFlags presence)
        {
            Main = main;
            Adjacent0 = adjacent0;
            Adjacent1 = adjacent1;
            Adjacent2 = adjacent2;
            Adjacent3 = adjacent3;
            Adjacent4 = adjacent4;
            Adjacent5 = adjacent5;
            Presence = presence;
        }

        /// <summary>
        /// Initializes a partial septuplet from stored values and per-position presence indicators.
        /// </summary>
        /// <param name="main">The value stored at the primary position.</param>
        /// <param name="adjacent0">The value stored at adjacent position 0.</param>
        /// <param name="adjacent1">The value stored at adjacent position 1.</param>
        /// <param name="adjacent2">The value stored at adjacent position 2.</param>
        /// <param name="adjacent3">The value stored at adjacent position 3.</param>
        /// <param name="adjacent4">The value stored at adjacent position 4.</param>
        /// <param name="adjacent5">The value stored at adjacent position 5.</param>
        /// <param name="hasMain">Whether the primary position is present.</param>
        /// <param name="hasAdjacent0">Whether adjacent position 0 is present.</param>
        /// <param name="hasAdjacent1">Whether adjacent position 1 is present.</param>
        /// <param name="hasAdjacent2">Whether adjacent position 2 is present.</param>
        /// <param name="hasAdjacent3">Whether adjacent position 3 is present.</param>
        /// <param name="hasAdjacent4">Whether adjacent position 4 is present.</param>
        /// <param name="hasAdjacent5">Whether adjacent position 5 is present.</param>
        public PartialSeptuplet(
            T main,
            T adjacent0,
            T adjacent1,
            T adjacent2,
            T adjacent3,
            T adjacent4,
            T adjacent5,
            bool hasMain,
            bool hasAdjacent0,
            bool hasAdjacent1,
            bool hasAdjacent2,
            bool hasAdjacent3,
            bool hasAdjacent4,
            bool hasAdjacent5)
            : this(
                main,
                adjacent0,
                adjacent1,
                adjacent2,
                adjacent3,
                adjacent4,
                adjacent5,
                CreatePresence(
                    hasMain,
                    hasAdjacent0,
                    hasAdjacent1,
                    hasAdjacent2,
                    hasAdjacent3,
                    hasAdjacent4,
                    hasAdjacent5))
        {
        }

        /// <summary>
        /// Initializes a partial septuplet from a complete septuplet and presence flags.
        /// </summary>
        /// <param name="septuplet">The complete septuplet whose values are stored.</param>
        /// <param name="presence">The positions that are semantically present.</param>
        public PartialSeptuplet(Septuplet<T> septuplet, SeptupletPresenceFlags presence)
            : this(
                septuplet.Main,
                septuplet.Adjacent0,
                septuplet.Adjacent1,
                septuplet.Adjacent2,
                septuplet.Adjacent3,
                septuplet.Adjacent4,
                septuplet.Adjacent5,
                presence)
        {
        }

        /// <summary>
        /// Gets the value stored at the primary position, regardless of presence.
        /// </summary>
        public T Main { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 0, regardless of presence.
        /// </summary>
        public T Adjacent0 { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 1, regardless of presence.
        /// </summary>
        public T Adjacent1 { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 2, regardless of presence.
        /// </summary>
        public T Adjacent2 { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 3, regardless of presence.
        /// </summary>
        public T Adjacent3 { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 4, regardless of presence.
        /// </summary>
        public T Adjacent4 { get; }

        /// <summary>
        /// Gets the value stored at adjacent position 5, regardless of presence.
        /// </summary>
        public T Adjacent5 { get; }

        /// <summary>
        /// Gets the flags identifying the semantically present positions.
        /// </summary>
        public SeptupletPresenceFlags Presence { get; }

        /// <summary>
        /// Gets whether the primary position is present.
        /// </summary>
        public bool HasMain => (Presence & SeptupletPresenceFlags.Main) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 0 is present.
        /// </summary>
        public bool HasAdjacent0 => (Presence & SeptupletPresenceFlags.Adjacent0) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 1 is present.
        /// </summary>
        public bool HasAdjacent1 => (Presence & SeptupletPresenceFlags.Adjacent1) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 2 is present.
        /// </summary>
        public bool HasAdjacent2 => (Presence & SeptupletPresenceFlags.Adjacent2) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 3 is present.
        /// </summary>
        public bool HasAdjacent3 => (Presence & SeptupletPresenceFlags.Adjacent3) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 4 is present.
        /// </summary>
        public bool HasAdjacent4 => (Presence & SeptupletPresenceFlags.Adjacent4) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Gets whether adjacent position 5 is present.
        /// </summary>
        public bool HasAdjacent5 => (Presence & SeptupletPresenceFlags.Adjacent5) != SeptupletPresenceFlags.None;

        /// <summary>
        /// Returns the stored values as a complete septuplet, discarding presence information.
        /// </summary>
        public Septuplet<T> ToSeptuplet()
        {
            return new Septuplet<T>(
                Main,
                Adjacent0,
                Adjacent1,
                Adjacent2,
                Adjacent3,
                Adjacent4,
                Adjacent5);
        }

        /// <summary>
        /// Deconstructs the stored values in main-then-adjacency order.
        /// </summary>
        /// <param name="main">Receives the stored primary value.</param>
        /// <param name="adjacent0">Receives the value at adjacent position 0.</param>
        /// <param name="adjacent1">Receives the value at adjacent position 1.</param>
        /// <param name="adjacent2">Receives the value at adjacent position 2.</param>
        /// <param name="adjacent3">Receives the value at adjacent position 3.</param>
        /// <param name="adjacent4">Receives the value at adjacent position 4.</param>
        /// <param name="adjacent5">Receives the value at adjacent position 5.</param>
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

        /// <summary>
        /// Deconstructs the stored values and their presence flags.
        /// </summary>
        /// <param name="main">Receives the stored primary value.</param>
        /// <param name="adjacent0">Receives the value at adjacent position 0.</param>
        /// <param name="adjacent1">Receives the value at adjacent position 1.</param>
        /// <param name="adjacent2">Receives the value at adjacent position 2.</param>
        /// <param name="adjacent3">Receives the value at adjacent position 3.</param>
        /// <param name="adjacent4">Receives the value at adjacent position 4.</param>
        /// <param name="adjacent5">Receives the value at adjacent position 5.</param>
        /// <param name="presence">Receives the present-position flags.</param>
        public void Deconstruct(
            out T main,
            out T adjacent0,
            out T adjacent1,
            out T adjacent2,
            out T adjacent3,
            out T adjacent4,
            out T adjacent5,
            out SeptupletPresenceFlags presence)
        {
            main = Main;
            adjacent0 = Adjacent0;
            adjacent1 = Adjacent1;
            adjacent2 = Adjacent2;
            adjacent3 = Adjacent3;
            adjacent4 = Adjacent4;
            adjacent5 = Adjacent5;
            presence = Presence;
        }

        private static SeptupletPresenceFlags CreatePresence(
            bool hasMain,
            bool hasAdjacent0,
            bool hasAdjacent1,
            bool hasAdjacent2,
            bool hasAdjacent3,
            bool hasAdjacent4,
            bool hasAdjacent5)
        {
            SeptupletPresenceFlags presence = SeptupletPresenceFlags.None;

            if (hasMain)
                presence |= SeptupletPresenceFlags.Main;

            if (hasAdjacent0)
                presence |= SeptupletPresenceFlags.Adjacent0;

            if (hasAdjacent1)
                presence |= SeptupletPresenceFlags.Adjacent1;

            if (hasAdjacent2)
                presence |= SeptupletPresenceFlags.Adjacent2;

            if (hasAdjacent3)
                presence |= SeptupletPresenceFlags.Adjacent3;

            if (hasAdjacent4)
                presence |= SeptupletPresenceFlags.Adjacent4;

            if (hasAdjacent5)
                presence |= SeptupletPresenceFlags.Adjacent5;

            return presence;
        }
    }
}
