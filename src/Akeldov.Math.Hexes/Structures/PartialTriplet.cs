namespace Akeldov.Math.Hexes.Topology
{
    public readonly struct PartialTriplet<T>
    {
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

        public PartialTriplet(Triplet<T> triplet, TripletPresenceFlags presence)
            : this(triplet.Main, triplet.Left, triplet.Right, presence)
        {
        }

        public T Main { get; }

        public T Left { get; }

        public T Right { get; }

        public TripletPresenceFlags Presence { get; }

        public bool HasMain => (Presence & TripletPresenceFlags.Main) != 0;

        public bool HasLeft => (Presence & TripletPresenceFlags.Left) != 0;

        public bool HasRight => (Presence & TripletPresenceFlags.Right) != 0;

        public Triplet<T> ToTriplet()
        {
            return new Triplet<T>(Main, Left, Right);
        }

        public void Deconstruct(out T main, out T left, out T right)
        {
            main = Main;
            left = Left;
            right = Right;
        }

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
