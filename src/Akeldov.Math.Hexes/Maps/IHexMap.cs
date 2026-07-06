using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines a contract for IHexMap implementations.
    /// </summary>
    /// <typeparam name="TValue">The type of value handled by this member.</typeparam>
    public interface IHexMap<TValue>
    {
        /// <summary>
        /// Represents the <c>Width</c> value.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Represents the <c>Height</c> value.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Represents the <c>Layout</c> value.
        /// </summary>
        Layout Layout { get; }

        /// <summary>
        /// Represents the <c>this[VectorXYInt]</c> value.
        /// </summary>
        /// <param name="index">The index value.</param>
        TValue this[VectorXYInt index] { get; }

        /// <summary>
        /// Represents the <c>this[int]</c> value.
        /// </summary>
        /// <param name="index">The index value.</param>
        TValue this[int index] { get; }
    }
}
