namespace Akeldov.Math.Hexes.Topology
{
    public readonly struct PartialSeptuplet<T>
    {
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

        public T Main { get; }

        public T Adjacent0 { get; }

        public T Adjacent1 { get; }

        public T Adjacent2 { get; }

        public T Adjacent3 { get; }

        public T Adjacent4 { get; }

        public T Adjacent5 { get; }

        public SeptupletPresenceFlags Presence { get; }

        public bool HasMain => (Presence & SeptupletPresenceFlags.Main) != 0;

        public bool HasAdjacent0 => (Presence & SeptupletPresenceFlags.Adjacent0) != 0;

        public bool HasAdjacent1 => (Presence & SeptupletPresenceFlags.Adjacent1) != 0;

        public bool HasAdjacent2 => (Presence & SeptupletPresenceFlags.Adjacent2) != 0;

        public bool HasAdjacent3 => (Presence & SeptupletPresenceFlags.Adjacent3) != 0;

        public bool HasAdjacent4 => (Presence & SeptupletPresenceFlags.Adjacent4) != 0;

        public bool HasAdjacent5 => (Presence & SeptupletPresenceFlags.Adjacent5) != 0;

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
