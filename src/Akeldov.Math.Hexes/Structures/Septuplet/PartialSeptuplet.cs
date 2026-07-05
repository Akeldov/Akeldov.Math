namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents a PartialSeptuplet value.
    /// </summary>
    /// <typeparam name="T">The type of value handled by this member.</typeparam>
    public readonly struct PartialSeptuplet<T>
    {
        /// <summary>
        /// Performs the PartialSeptuplet operation.
        /// </summary>
        /// <param name="main">The Main value.</param>
        /// <param name="adjacent0">The Adjacent0 value.</param>
        /// <param name="adjacent1">The Adjacent1 value.</param>
        /// <param name="adjacent2">The Adjacent2 value.</param>
        /// <param name="adjacent3">The Adjacent3 value.</param>
        /// <param name="adjacent4">The Adjacent4 value.</param>
        /// <param name="adjacent5">The Adjacent5 value.</param>
        /// <param name="presence">The Presence value.</param>
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
        /// Performs the PartialSeptuplet operation.
        /// </summary>
        /// <param name="hasMain">The HasMain value.</param>
        /// <param name="hasAdjacent0">The HasAdjacent0 value.</param>
        /// <param name="hasAdjacent1">The HasAdjacent1 value.</param>
        /// <param name="hasAdjacent2">The HasAdjacent2 value.</param>
        /// <param name="hasAdjacent3">The HasAdjacent3 value.</param>
        /// <param name="hasAdjacent4">The HasAdjacent4 value.</param>
        /// <param name="hasAdjacent5">The HasAdjacent5 value.</param>
        /// <param name="main">The Main value.</param>
        /// <param name="adjacent0">The Adjacent0 value.</param>
        /// <param name="adjacent1">The Adjacent1 value.</param>
        /// <param name="adjacent2">The Adjacent2 value.</param>
        /// <param name="adjacent3">The Adjacent3 value.</param>
        /// <param name="adjacent4">The Adjacent4 value.</param>
        /// <param name="adjacent5">The Adjacent5 value.</param>
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
        /// Performs the PartialSeptuplet operation.
        /// </summary>
        /// <param name="presence">The presence value.</param>
        /// <param name="septuplet">The Septuplet value.</param>
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
        /// Gets the Presence value.
        /// </summary>
        public SeptupletPresenceFlags Presence { get; }

        /// <summary>
        /// Performs the HasMain operation.
        /// </summary>
        public bool HasMain => (Presence & SeptupletPresenceFlags.Main) != 0;

        /// <summary>
        /// Performs the HasAdjacent0 operation.
        /// </summary>
        public bool HasAdjacent0 => (Presence & SeptupletPresenceFlags.Adjacent0) != 0;

        /// <summary>
        /// Performs the HasAdjacent1 operation.
        /// </summary>
        public bool HasAdjacent1 => (Presence & SeptupletPresenceFlags.Adjacent1) != 0;

        /// <summary>
        /// Performs the HasAdjacent2 operation.
        /// </summary>
        public bool HasAdjacent2 => (Presence & SeptupletPresenceFlags.Adjacent2) != 0;

        /// <summary>
        /// Performs the HasAdjacent3 operation.
        /// </summary>
        public bool HasAdjacent3 => (Presence & SeptupletPresenceFlags.Adjacent3) != 0;

        /// <summary>
        /// Performs the HasAdjacent4 operation.
        /// </summary>
        public bool HasAdjacent4 => (Presence & SeptupletPresenceFlags.Adjacent4) != 0;

        /// <summary>
        /// Performs the HasAdjacent5 operation.
        /// </summary>
        public bool HasAdjacent5 => (Presence & SeptupletPresenceFlags.Adjacent5) != 0;

        /// <summary>
        /// Converts the value to the requested representation.
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
        /// <param name="presence">The Presence value.</param>
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
