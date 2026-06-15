using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Represents an 8-bit RGBA color.
    /// </summary>
    public readonly struct RGBA8BitColor : IEquatable<RGBA8BitColor>
    {
        internal static readonly RGBA8BitColor[] TemperaturePalette =
        {
            new RGBA8BitColor(0, 0, 255, 255),
            new RGBA8BitColor(0, 4, 255, 255),
            new RGBA8BitColor(0, 8, 255, 255),
            new RGBA8BitColor(0, 12, 255, 255),
            new RGBA8BitColor(0, 16, 255, 255),
            new RGBA8BitColor(0, 20, 255, 255),
            new RGBA8BitColor(0, 24, 255, 255),
            new RGBA8BitColor(0, 28, 255, 255),
            new RGBA8BitColor(0, 32, 255, 255),
            new RGBA8BitColor(0, 36, 255, 255),
            new RGBA8BitColor(0, 40, 255, 255),
            new RGBA8BitColor(0, 44, 255, 255),
            new RGBA8BitColor(0, 48, 255, 255),
            new RGBA8BitColor(0, 52, 255, 255),
            new RGBA8BitColor(0, 56, 255, 255),
            new RGBA8BitColor(0, 60, 255, 255),
            new RGBA8BitColor(0, 64, 255, 255),
            new RGBA8BitColor(0, 68, 255, 255),
            new RGBA8BitColor(0, 72, 255, 255),
            new RGBA8BitColor(0, 76, 255, 255),
            new RGBA8BitColor(0, 80, 255, 255),
            new RGBA8BitColor(0, 84, 255, 255),
            new RGBA8BitColor(0, 88, 255, 255),
            new RGBA8BitColor(0, 92, 255, 255),
            new RGBA8BitColor(0, 96, 255, 255),
            new RGBA8BitColor(0, 100, 255, 255),
            new RGBA8BitColor(0, 104, 255, 255),
            new RGBA8BitColor(0, 108, 255, 255),
            new RGBA8BitColor(0, 112, 255, 255),
            new RGBA8BitColor(0, 116, 255, 255),
            new RGBA8BitColor(0, 120, 255, 255),
            new RGBA8BitColor(0, 124, 255, 255),
            new RGBA8BitColor(0, 128, 255, 255),
            new RGBA8BitColor(0, 131, 255, 255),
            new RGBA8BitColor(0, 135, 255, 255),
            new RGBA8BitColor(0, 139, 255, 255),
            new RGBA8BitColor(0, 143, 255, 255),
            new RGBA8BitColor(0, 147, 255, 255),
            new RGBA8BitColor(0, 151, 255, 255),
            new RGBA8BitColor(0, 155, 255, 255),
            new RGBA8BitColor(0, 159, 255, 255),
            new RGBA8BitColor(0, 163, 255, 255),
            new RGBA8BitColor(0, 167, 255, 255),
            new RGBA8BitColor(0, 171, 255, 255),
            new RGBA8BitColor(0, 175, 255, 255),
            new RGBA8BitColor(0, 179, 255, 255),
            new RGBA8BitColor(0, 183, 255, 255),
            new RGBA8BitColor(0, 187, 255, 255),
            new RGBA8BitColor(0, 191, 255, 255),
            new RGBA8BitColor(0, 195, 255, 255),
            new RGBA8BitColor(0, 199, 255, 255),
            new RGBA8BitColor(0, 203, 255, 255),
            new RGBA8BitColor(0, 207, 255, 255),
            new RGBA8BitColor(0, 211, 255, 255),
            new RGBA8BitColor(0, 215, 255, 255),
            new RGBA8BitColor(0, 219, 255, 255),
            new RGBA8BitColor(0, 223, 255, 255),
            new RGBA8BitColor(0, 227, 255, 255),
            new RGBA8BitColor(0, 231, 255, 255),
            new RGBA8BitColor(0, 235, 255, 255),
            new RGBA8BitColor(0, 239, 255, 255),
            new RGBA8BitColor(0, 243, 255, 255),
            new RGBA8BitColor(0, 247, 255, 255),
            new RGBA8BitColor(0, 251, 255, 255),
            new RGBA8BitColor(0, 255, 255, 255),
            new RGBA8BitColor(0, 255, 251, 255),
            new RGBA8BitColor(0, 255, 247, 255),
            new RGBA8BitColor(0, 255, 243, 255),
            new RGBA8BitColor(0, 255, 239, 255),
            new RGBA8BitColor(0, 255, 235, 255),
            new RGBA8BitColor(0, 255, 231, 255),
            new RGBA8BitColor(0, 255, 227, 255),
            new RGBA8BitColor(0, 255, 223, 255),
            new RGBA8BitColor(0, 255, 219, 255),
            new RGBA8BitColor(0, 255, 215, 255),
            new RGBA8BitColor(0, 255, 211, 255),
            new RGBA8BitColor(0, 255, 207, 255),
            new RGBA8BitColor(0, 255, 203, 255),
            new RGBA8BitColor(0, 255, 199, 255),
            new RGBA8BitColor(0, 255, 195, 255),
            new RGBA8BitColor(0, 255, 191, 255),
            new RGBA8BitColor(0, 255, 187, 255),
            new RGBA8BitColor(0, 255, 183, 255),
            new RGBA8BitColor(0, 255, 179, 255),
            new RGBA8BitColor(0, 255, 175, 255),
            new RGBA8BitColor(0, 255, 171, 255),
            new RGBA8BitColor(0, 255, 167, 255),
            new RGBA8BitColor(0, 255, 163, 255),
            new RGBA8BitColor(0, 255, 159, 255),
            new RGBA8BitColor(0, 255, 155, 255),
            new RGBA8BitColor(0, 255, 151, 255),
            new RGBA8BitColor(0, 255, 147, 255),
            new RGBA8BitColor(0, 255, 143, 255),
            new RGBA8BitColor(0, 255, 139, 255),
            new RGBA8BitColor(0, 255, 135, 255),
            new RGBA8BitColor(0, 255, 131, 255),
            new RGBA8BitColor(0, 255, 128, 255),
            new RGBA8BitColor(0, 255, 124, 255),
            new RGBA8BitColor(0, 255, 120, 255),
            new RGBA8BitColor(0, 255, 116, 255),
            new RGBA8BitColor(0, 255, 112, 255),
            new RGBA8BitColor(0, 255, 108, 255),
            new RGBA8BitColor(0, 255, 104, 255),
            new RGBA8BitColor(0, 255, 100, 255),
            new RGBA8BitColor(0, 255, 96, 255),
            new RGBA8BitColor(0, 255, 92, 255),
            new RGBA8BitColor(0, 255, 88, 255),
            new RGBA8BitColor(0, 255, 84, 255),
            new RGBA8BitColor(0, 255, 80, 255),
            new RGBA8BitColor(0, 255, 76, 255),
            new RGBA8BitColor(0, 255, 72, 255),
            new RGBA8BitColor(0, 255, 68, 255),
            new RGBA8BitColor(0, 255, 64, 255),
            new RGBA8BitColor(0, 255, 60, 255),
            new RGBA8BitColor(0, 255, 56, 255),
            new RGBA8BitColor(0, 255, 52, 255),
            new RGBA8BitColor(0, 255, 48, 255),
            new RGBA8BitColor(0, 255, 44, 255),
            new RGBA8BitColor(0, 255, 40, 255),
            new RGBA8BitColor(0, 255, 36, 255),
            new RGBA8BitColor(0, 255, 32, 255),
            new RGBA8BitColor(0, 255, 28, 255),
            new RGBA8BitColor(0, 255, 24, 255),
            new RGBA8BitColor(0, 255, 20, 255),
            new RGBA8BitColor(0, 255, 16, 255),
            new RGBA8BitColor(0, 255, 12, 255),
            new RGBA8BitColor(0, 255, 8, 255),
            new RGBA8BitColor(0, 255, 4, 255),
            new RGBA8BitColor(0, 255, 0, 255),
            new RGBA8BitColor(4, 255, 0, 255),
            new RGBA8BitColor(8, 255, 0, 255),
            new RGBA8BitColor(12, 255, 0, 255),
            new RGBA8BitColor(16, 255, 0, 255),
            new RGBA8BitColor(20, 255, 0, 255),
            new RGBA8BitColor(24, 255, 0, 255),
            new RGBA8BitColor(28, 255, 0, 255),
            new RGBA8BitColor(32, 255, 0, 255),
            new RGBA8BitColor(36, 255, 0, 255),
            new RGBA8BitColor(40, 255, 0, 255),
            new RGBA8BitColor(45, 255, 0, 255),
            new RGBA8BitColor(49, 255, 0, 255),
            new RGBA8BitColor(53, 255, 0, 255),
            new RGBA8BitColor(57, 255, 0, 255),
            new RGBA8BitColor(61, 255, 0, 255),
            new RGBA8BitColor(65, 255, 0, 255),
            new RGBA8BitColor(69, 255, 0, 255),
            new RGBA8BitColor(73, 255, 0, 255),
            new RGBA8BitColor(77, 255, 0, 255),
            new RGBA8BitColor(81, 255, 0, 255),
            new RGBA8BitColor(85, 255, 0, 255),
            new RGBA8BitColor(89, 255, 0, 255),
            new RGBA8BitColor(93, 255, 0, 255),
            new RGBA8BitColor(97, 255, 0, 255),
            new RGBA8BitColor(101, 255, 0, 255),
            new RGBA8BitColor(105, 255, 0, 255),
            new RGBA8BitColor(109, 255, 0, 255),
            new RGBA8BitColor(113, 255, 0, 255),
            new RGBA8BitColor(117, 255, 0, 255),
            new RGBA8BitColor(121, 255, 0, 255),
            new RGBA8BitColor(125, 255, 0, 255),
            new RGBA8BitColor(130, 255, 0, 255),
            new RGBA8BitColor(134, 255, 0, 255),
            new RGBA8BitColor(138, 255, 0, 255),
            new RGBA8BitColor(142, 255, 0, 255),
            new RGBA8BitColor(146, 255, 0, 255),
            new RGBA8BitColor(150, 255, 0, 255),
            new RGBA8BitColor(154, 255, 0, 255),
            new RGBA8BitColor(158, 255, 0, 255),
            new RGBA8BitColor(162, 255, 0, 255),
            new RGBA8BitColor(166, 255, 0, 255),
            new RGBA8BitColor(170, 255, 0, 255),
            new RGBA8BitColor(174, 255, 0, 255),
            new RGBA8BitColor(178, 255, 0, 255),
            new RGBA8BitColor(182, 255, 0, 255),
            new RGBA8BitColor(186, 255, 0, 255),
            new RGBA8BitColor(190, 255, 0, 255),
            new RGBA8BitColor(194, 255, 0, 255),
            new RGBA8BitColor(198, 255, 0, 255),
            new RGBA8BitColor(202, 255, 0, 255),
            new RGBA8BitColor(206, 255, 0, 255),
            new RGBA8BitColor(210, 255, 0, 255),
            new RGBA8BitColor(215, 255, 0, 255),
            new RGBA8BitColor(219, 255, 0, 255),
            new RGBA8BitColor(223, 255, 0, 255),
            new RGBA8BitColor(227, 255, 0, 255),
            new RGBA8BitColor(231, 255, 0, 255),
            new RGBA8BitColor(235, 255, 0, 255),
            new RGBA8BitColor(239, 255, 0, 255),
            new RGBA8BitColor(243, 255, 0, 255),
            new RGBA8BitColor(247, 255, 0, 255),
            new RGBA8BitColor(251, 255, 0, 255),
            new RGBA8BitColor(255, 255, 0, 255),
            new RGBA8BitColor(255, 251, 0, 255),
            new RGBA8BitColor(255, 247, 0, 255),
            new RGBA8BitColor(255, 243, 0, 255),
            new RGBA8BitColor(255, 239, 0, 255),
            new RGBA8BitColor(255, 235, 0, 255),
            new RGBA8BitColor(255, 231, 0, 255),
            new RGBA8BitColor(255, 227, 0, 255),
            new RGBA8BitColor(255, 223, 0, 255),
            new RGBA8BitColor(255, 219, 0, 255),
            new RGBA8BitColor(255, 215, 0, 255),
            new RGBA8BitColor(255, 211, 0, 255),
            new RGBA8BitColor(255, 207, 0, 255),
            new RGBA8BitColor(255, 203, 0, 255),
            new RGBA8BitColor(255, 199, 0, 255),
            new RGBA8BitColor(255, 195, 0, 255),
            new RGBA8BitColor(255, 191, 0, 255),
            new RGBA8BitColor(255, 187, 0, 255),
            new RGBA8BitColor(255, 183, 0, 255),
            new RGBA8BitColor(255, 179, 0, 255),
            new RGBA8BitColor(255, 175, 0, 255),
            new RGBA8BitColor(255, 171, 0, 255),
            new RGBA8BitColor(255, 167, 0, 255),
            new RGBA8BitColor(255, 163, 0, 255),
            new RGBA8BitColor(255, 159, 0, 255),
            new RGBA8BitColor(255, 155, 0, 255),
            new RGBA8BitColor(255, 151, 0, 255),
            new RGBA8BitColor(255, 147, 0, 255),
            new RGBA8BitColor(255, 143, 0, 255),
            new RGBA8BitColor(255, 139, 0, 255),
            new RGBA8BitColor(255, 135, 0, 255),
            new RGBA8BitColor(255, 131, 0, 255),
            new RGBA8BitColor(255, 128, 0, 255),
            new RGBA8BitColor(255, 124, 0, 255),
            new RGBA8BitColor(255, 120, 0, 255),
            new RGBA8BitColor(255, 116, 0, 255),
            new RGBA8BitColor(255, 112, 0, 255),
            new RGBA8BitColor(255, 108, 0, 255),
            new RGBA8BitColor(255, 104, 0, 255),
            new RGBA8BitColor(255, 100, 0, 255),
            new RGBA8BitColor(255, 96, 0, 255),
            new RGBA8BitColor(255, 92, 0, 255),
            new RGBA8BitColor(255, 88, 0, 255),
            new RGBA8BitColor(255, 84, 0, 255),
            new RGBA8BitColor(255, 80, 0, 255),
            new RGBA8BitColor(255, 76, 0, 255),
            new RGBA8BitColor(255, 72, 0, 255),
            new RGBA8BitColor(255, 68, 0, 255),
            new RGBA8BitColor(255, 64, 0, 255),
            new RGBA8BitColor(255, 60, 0, 255),
            new RGBA8BitColor(255, 56, 0, 255),
            new RGBA8BitColor(255, 52, 0, 255),
            new RGBA8BitColor(255, 48, 0, 255),
            new RGBA8BitColor(255, 44, 0, 255),
            new RGBA8BitColor(255, 40, 0, 255),
            new RGBA8BitColor(255, 36, 0, 255),
            new RGBA8BitColor(255, 32, 0, 255),
            new RGBA8BitColor(255, 28, 0, 255),
            new RGBA8BitColor(255, 24, 0, 255),
            new RGBA8BitColor(255, 20, 0, 255),
            new RGBA8BitColor(255, 16, 0, 255),
            new RGBA8BitColor(255, 12, 0, 255),
            new RGBA8BitColor(255, 8, 0, 255),
            new RGBA8BitColor(255, 4, 0, 255),
            new RGBA8BitColor(255, 0, 0, 255),
        };

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
        /// Creates an 8-bit RGBA temperature-style color from a normalized value.
        /// </summary>
        /// <param name="normalizedValue">The normalized value. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>An 8-bit RGBA color on the blue-cyan-green-yellow-red temperature scale.</returns>
        public static RGBA8BitColor FromTemperature(float normalizedValue)
        {
            if (float.IsNaN(normalizedValue) || float.IsInfinity(normalizedValue))
                throw new ArgumentOutOfRangeException(nameof(normalizedValue), "Normalized temperature value must be finite.");

            int index = ToTemperaturePaletteIndex(normalizedValue);
            return TemperaturePalette[index];
        }

        /// <summary>
        /// Creates an 8-bit RGBA temperature-style color from a value and its value range.
        /// </summary>
        /// <param name="value">The value to map to the temperature color scale.</param>
        /// <param name="min">The range minimum value.</param>
        /// <param name="max">The range maximum value.</param>
        /// <returns>
        /// An 8-bit RGBA color on the blue-cyan-green-yellow-red temperature scale, where lower values are blue
        /// and higher values are red.
        /// </returns>
        public static RGBA8BitColor FromTemperature(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Temperature value must be finite.");

            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentOutOfRangeException(nameof(min), "Temperature range values must be finite.");

            if (float.IsNaN(max) || float.IsInfinity(max) || max < min)
                throw new ArgumentOutOfRangeException(nameof(max), "Temperature maximum value must be finite and greater than or equal to the minimum value.");

            if (min == max)
                return FromTemperature(0.5f);

            return FromTemperature((value - min) / (max - min));
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

        private static int ToTemperaturePaletteIndex(float normalizedValue)
        {
            normalizedValue = MathF.Min(MathF.Max(normalizedValue, 0f), 1f);
            return (int)MathF.Round(normalizedValue * (TemperaturePalette.Length - 1));
        }
    }
}
