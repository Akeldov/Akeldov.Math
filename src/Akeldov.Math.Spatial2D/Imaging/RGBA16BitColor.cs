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
        /// Creates a 16-bit RGBA color from normalized channel values.
        /// </summary>
        /// <param name="red">The normalized red channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="green">The normalized green channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="blue">The normalized blue channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <param name="alpha">The normalized alpha channel value. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>A 16-bit RGBA color with channels converted from the normalized values.</returns>
        public static RGBA16BitColor FromNormalized(
            float red,
            float green,
            float blue,
            float alpha = 1f)
        {
            return new RGBA16BitColor(
                ToChannel(red, nameof(red)),
                ToChannel(green, nameof(green)),
                ToChannel(blue, nameof(blue)),
                ToChannel(alpha, nameof(alpha)));
        }

        /// <summary>
        /// Linearly blends two 16-bit RGBA colors.
        /// </summary>
        /// <param name="from">The color returned when <paramref name="amount"/> is 0.</param>
        /// <param name="to">The color returned when <paramref name="amount"/> is 1.</param>
        /// <param name="amount">The blend amount. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>The linearly blended color.</returns>
        public static RGBA16BitColor Blend(RGBA16BitColor from, RGBA16BitColor to, float amount)
        {
            amount = ClampNormalized(amount, nameof(amount));
            float inverseAmount = 1f - amount;

            return new RGBA16BitColor(
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

        private static ushort ToChannel(float value, string paramName)
        {
            value = ClampNormalized(value, paramName);
            return (ushort)MathF.Round(value * ushort.MaxValue);
        }

        private static float ClampNormalized(float value, string paramName)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException(paramName, "Normalized channel values and blend amounts must not be NaN.");

            return MathF.Min(MathF.Max(value, 0f), 1f);
        }

        private static ushort BlendChannel(ushort from, ushort to, float amount, float inverseAmount)
        {
            return (ushort)MathF.Round(from * inverseAmount + to * amount);
        }
    }
}
