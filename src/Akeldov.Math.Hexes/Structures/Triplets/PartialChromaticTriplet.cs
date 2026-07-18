namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Represents three values ordered by chromatic index with per-position presence information.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public readonly struct PartialChromaticTriplet<T>
    {
        /// <summary>
        /// Initializes a partial chromatically ordered triplet.
        /// </summary>
        /// <param name="index0">The value associated with chromatic index zero.</param>
        /// <param name="index1">The value associated with chromatic index one.</param>
        /// <param name="index2">The value associated with chromatic index two.</param>
        /// <param name="presence">The present chromatic-index positions.</param>
        public PartialChromaticTriplet(
            T index0,
            T index1,
            T index2,
            ChromaticTripletPresenceFlags presence)
        {
            Index0 = index0;
            Index1 = index1;
            Index2 = index2;
            Presence = presence;
        }

        /// <summary>
        /// Initializes a partial chromatically ordered triplet.
        /// </summary>
        /// <param name="index0">The value associated with chromatic index zero.</param>
        /// <param name="index1">The value associated with chromatic index one.</param>
        /// <param name="index2">The value associated with chromatic index two.</param>
        /// <param name="hasIndex0">Whether the position for chromatic index zero is present.</param>
        /// <param name="hasIndex1">Whether the position for chromatic index one is present.</param>
        /// <param name="hasIndex2">Whether the position for chromatic index two is present.</param>
        public PartialChromaticTriplet(
            T index0,
            T index1,
            T index2,
            bool hasIndex0,
            bool hasIndex1,
            bool hasIndex2)
            : this(index0, index1, index2, CreatePresence(hasIndex0, hasIndex1, hasIndex2))
        {
        }

        /// <summary>
        /// Initializes a partial triplet from a complete chromatic triplet and presence flags.
        /// </summary>
        /// <param name="triplet">The complete chromatically ordered triplet.</param>
        /// <param name="presence">The present chromatic-index positions.</param>
        public PartialChromaticTriplet(
            ChromaticTriplet<T> triplet,
            ChromaticTripletPresenceFlags presence)
            : this(triplet.Index0, triplet.Index1, triplet.Index2, presence)
        {
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
        /// Gets the present chromatic-index positions.
        /// </summary>
        public ChromaticTripletPresenceFlags Presence { get; }

        /// <summary>
        /// Gets whether the position for chromatic index zero is present.
        /// </summary>
        public bool HasIndex0 => (Presence & ChromaticTripletPresenceFlags.Index0) != 0;

        /// <summary>
        /// Gets whether the position for chromatic index one is present.
        /// </summary>
        public bool HasIndex1 => (Presence & ChromaticTripletPresenceFlags.Index1) != 0;

        /// <summary>
        /// Gets whether the position for chromatic index two is present.
        /// </summary>
        public bool HasIndex2 => (Presence & ChromaticTripletPresenceFlags.Index2) != 0;

        /// <summary>
        /// Returns the values without presence information.
        /// </summary>
        /// <returns>A complete chromatically ordered triplet containing the stored values.</returns>
        public ChromaticTriplet<T> ToTriplet()
        {
            return new ChromaticTriplet<T>(Index0, Index1, Index2);
        }

        /// <summary>
        /// Deconstructs the values in chromatic-index order.
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

        /// <summary>
        /// Deconstructs the values and presence flags in chromatic-index order.
        /// </summary>
        /// <param name="index0">The value associated with chromatic index zero.</param>
        /// <param name="index1">The value associated with chromatic index one.</param>
        /// <param name="index2">The value associated with chromatic index two.</param>
        /// <param name="presence">The present chromatic-index positions.</param>
        public void Deconstruct(
            out T index0,
            out T index1,
            out T index2,
            out ChromaticTripletPresenceFlags presence)
        {
            index0 = Index0;
            index1 = Index1;
            index2 = Index2;
            presence = Presence;
        }

        private static ChromaticTripletPresenceFlags CreatePresence(
            bool hasIndex0,
            bool hasIndex1,
            bool hasIndex2)
        {
            ChromaticTripletPresenceFlags presence = ChromaticTripletPresenceFlags.None;

            if (hasIndex0)
                presence |= ChromaticTripletPresenceFlags.Index0;

            if (hasIndex1)
                presence |= ChromaticTripletPresenceFlags.Index1;

            if (hasIndex2)
                presence |= ChromaticTripletPresenceFlags.Index2;

            return presence;
        }
    }
}
