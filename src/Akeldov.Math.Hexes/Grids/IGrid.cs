using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    public interface IGrid<TValue>
    {
        int Width { get; }

        int Height { get; }

        TValue this[VectorXYInt index] { get; }

        TValue this[int index] { get; }
    }
}
