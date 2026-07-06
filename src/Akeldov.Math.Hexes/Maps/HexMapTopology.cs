using System;
using System.Globalization;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Describes the dimensions and layout of a rectangular hex map.
    /// </summary>
    public readonly struct HexMapTopology : IEquatable<HexMapTopology>
    {
        /// <summary>
        /// Initializes a new hex map topology.
        /// </summary>
        /// <param name="width">The map width in hexes.</param>
        /// <param name="height">The map height in hexes.</param>
        /// <param name="layout">The hex layout used by the map.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="height"/> is negative, or
        /// when <paramref name="layout"/> is not supported.
        /// </exception>
        /// <exception cref="OverflowException">Thrown when the cell count does not fit <see cref="int"/>.</exception>
        public HexMapTopology(int width, int height, Layout layout)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                case Layout.OddQ:
                case Layout.EvenQ:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }

            _ = checked(width * height);

            Width = width;
            Height = height;
            Layout = layout;
        }

        /// <summary>
        /// Gets the map width in hexes.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the map height in hexes.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the total number of cells in the map.
        /// </summary>
        public int Count => checked(Width * Height);

        /// <summary>
        /// Gets the hex layout used by the map.
        /// </summary>
        public Layout Layout { get; }

        /// <summary>
        /// Indicates whether this topology has the same dimensions and layout as another topology.
        /// </summary>
        /// <param name="other">The topology to compare with this topology.</param>
        /// <returns><see langword="true"/> if both topologies are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(HexMapTopology other) =>
            Width == other.Width &&
            Height == other.Height &&
            Layout == other.Layout;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is HexMapTopology other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Width, Height, Layout);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "HexMapTopology(width: {0}, height: {1}, layout: {2})",
                Width,
                Height,
                Layout);

        /// <summary>
        /// Deconstructs this topology into its width, height, and layout.
        /// </summary>
        /// <param name="width">The map width in hexes.</param>
        /// <param name="height">The map height in hexes.</param>
        /// <param name="layout">The hex layout used by the map.</param>
        public void Deconstruct(out int width, out int height, out Layout layout)
        {
            width = Width;
            height = Height;
            layout = Layout;
        }

        /// <summary>
        /// Indicates whether two topologies are equal.
        /// </summary>
        /// <param name="left">The first topology.</param>
        /// <param name="right">The second topology.</param>
        /// <returns><see langword="true"/> if both topologies are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(HexMapTopology left, HexMapTopology right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two topologies are different.
        /// </summary>
        /// <param name="left">The first topology.</param>
        /// <param name="right">The second topology.</param>
        /// <returns><see langword="true"/> if the topologies differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(HexMapTopology left, HexMapTopology right) => !left.Equals(right);
    }
}
