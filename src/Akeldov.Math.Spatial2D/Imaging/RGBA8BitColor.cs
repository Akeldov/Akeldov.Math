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
        /// Creates an 8-bit RGBA color from normalized channel values.
        /// </summary>
        /// <param name="red">The normalized red channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="green">The normalized green channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="blue">The normalized blue channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="alpha">The normalized alpha channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>An 8-bit RGBA color with channels converted from the normalized values.</returns>
        public static RGBA8BitColor FromNormalized(
            float red,
            float green,
            float blue,
            float alpha = 1f)
        {
            return new RGBA8BitColor(
                ToChannel(red, nameof(red)),
                ToChannel(green, nameof(green)),
                ToChannel(blue, nameof(blue)),
                ToChannel(alpha, nameof(alpha)));
        }

        /// <summary>
        /// Linearly blends two 8-bit RGBA colors.
        /// </summary>
        /// <param name="from">The color returned when <paramref name="amount"/> is 0.</param>
        /// <param name="to">The color returned when <paramref name="amount"/> is 1.</param>
        /// <param name="amount">The blend amount. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>The linearly blended color.</returns>
        public static RGBA8BitColor Blend(RGBA8BitColor from, RGBA8BitColor to, float amount)
        {
            amount = ClampNormalized(amount, nameof(amount));
            float inverseAmount = 1f - amount;

            return new RGBA8BitColor(
                BlendChannel(from.Red, to.Red, amount, inverseAmount),
                BlendChannel(from.Green, to.Green, amount, inverseAmount),
                BlendChannel(from.Blue, to.Blue, amount, inverseAmount),
                BlendChannel(from.Alpha, to.Alpha, amount, inverseAmount));
        }

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

        private static byte ToChannel(float value, string paramName)
        {
            value = ClampNormalized(value, paramName);
            return (byte)MathF.Round(value * byte.MaxValue);
        }

        private static float ClampNormalized(float value, string paramName)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException(paramName, "Normalized channel values and blend amounts must not be NaN.");

            return MathF.Min(MathF.Max(value, 0f), 1f);
        }

        private static byte BlendChannel(byte from, byte to, float amount, float inverseAmount)
        {
            return (byte)MathF.Round(from * inverseAmount + to * amount);
        }
    }
}
