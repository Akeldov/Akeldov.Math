namespace Akeldov.Math.Hexes.Topology
{
    public readonly struct Pair<T>
    {
        public Pair(T left, T right)
        {
            Left = left;
            Right = right;
        }

        public T Left { get; }

        public T Right { get; }

        public void Deconstruct(out T left, out T right)
        {
            left = Left;
            right = Right;
        }
    }
}
