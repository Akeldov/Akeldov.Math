namespace Akeldov.Math.Hexes.Topology
{
    public readonly struct Triplet<T>
    {
        public Triplet(T main, T left, T right)
        {
            Main = main;
            Left = left;
            Right = right;
        }

        public T Main { get; }

        public T Left { get; }

        public T Right { get; }

        public void Deconstruct(out T main, out T left, out T right)
        {
            main = Main;
            left = Left;
            right = Right;
        }
    }
}
