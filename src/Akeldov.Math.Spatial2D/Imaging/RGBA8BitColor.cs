using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Represents an 8-bit RGBA color.
    /// </summary>
    public readonly struct RGBA8BitColor : IEquatable<RGBA8BitColor>
    {
        /// <summary>
        /// Initializes a new 8-bit RGBA color.
        /// </summary>
        /// <param name="red">The red channel value.</param>
        /// <param name="green">The green channel value.</param>
        /// <param name="blue">The blue channel value.</param>
        /// <param name="alpha">The alpha channel value.</param>
        public RGBA8BitColor(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        public byte Red { get; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        public byte Blue { get; }

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        public byte Alpha { get; }

        /// <summary>
        /// Indicates whether this color has the same channel values as another color.
        /// </summary>
        /// <param name="other">The color to compare with this color.</param>
        /// <returns><see langword="true"/> if all channel values are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RGBA8BitColor other) =>
            Red == other.Red &&
            Green == other.Green &&
            Blue == other.Blue &&
            Alpha == other.Alpha;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RGBA8BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "rgba8({0}, {1}, {2}, {3})", Red, Green, Blue, Alpha);

        /// <summary>
        /// Indicates whether two colors are equal.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns><see langword="true"/> if both colors are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RGBA8BitColor left, RGBA8BitColor right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two colors are different.
        /// </summary>
        /// <param name="left">The first color.</param>
        /// <param name="right">The second color.</param>
        /// <returns><see langword="true"/> if the colors are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RGBA8BitColor left, RGBA8BitColor right) => !(left == right);
    }
}
