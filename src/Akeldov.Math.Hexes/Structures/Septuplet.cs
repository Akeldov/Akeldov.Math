namespace Akeldov.Math.Hexes.Topology
{
    public readonly struct Septuplet<T>
    {
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

        public T Main { get; }

        public T Adjacent0 { get; }

        public T Adjacent1 { get; }

        public T Adjacent2 { get; }

        public T Adjacent3 { get; }

        public T Adjacent4 { get; }

        public T Adjacent5 { get; }

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
