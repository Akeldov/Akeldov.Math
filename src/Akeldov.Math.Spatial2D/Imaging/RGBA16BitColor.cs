using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Represents a 16-bit RGBA color.
    /// </summary>
    public readonly struct RGBA16BitColor : IEquatable<RGBA16BitColor>
    {
        /// <summary>
        /// Initializes a new 16-bit RGBA color.
        /// </summary>
        /// <param name="red">The red channel value.</param>
        /// <param name="green">The green channel value.</param>
        /// <param name="blue">The blue channel value.</param>
        /// <param name="alpha">The alpha channel value.</param>
        public RGBA16BitColor(ushort red, ushort green, ushort blue, ushort alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        public ushort Red { get; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        public ushort Green { get; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        public ushort Blue { get; }

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        public ushort Alpha { get; }

        /// <summary>
        /// Indicates whether this color has the same channel values as another color.
        /// </summary>
        /// <param name="other">The color to compare with this color.</param>
        /// <returns><see langword="true"/> if all channel values are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RGBA16BitColor other) =>
            Red == other.Red &&
            Green == other.Green &&
            Blue == other.Blue &&
            Alpha == other.Alpha;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RGBA16BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "rgba16({0}, {1}, {2}, {3})", Red, Green, Blue, Alpha);

        /// <summary>
        /// Indicates whether two colors are equal.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RGBA16BitColor left, RGBA16BitColor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two colors are different.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns><see langword="true"/> if the colors are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RGBA16BitColor left, RGBA16BitColor right) => !(left == right);
    }
}
