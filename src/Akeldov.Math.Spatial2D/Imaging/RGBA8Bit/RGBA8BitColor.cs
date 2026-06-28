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
            new RGBA8BitColor(0, 0, 128, 255),
            new RGBA8BitColor(0, 0, 132, 255),
            new RGBA8BitColor(0, 0, 136, 255),
            new RGBA8BitColor(0, 0, 140, 255),
            new RGBA8BitColor(0, 0, 144, 255),
            new RGBA8BitColor(0, 0, 147, 255),
            new RGBA8BitColor(0, 0, 152, 255),
            new RGBA8BitColor(0, 0, 156, 255),
            new RGBA8BitColor(0, 0, 160, 255),
            new RGBA8BitColor(0, 0, 163, 255),
            new RGBA8BitColor(0, 0, 168, 255),
            new RGBA8BitColor(0, 0, 172, 255),
            new RGBA8BitColor(0, 0, 176, 255),
            new RGBA8BitColor(0, 0, 179, 255),
            new RGBA8BitColor(0, 0, 184, 255),
            new RGBA8BitColor(0, 0, 188, 255),
            new RGBA8BitColor(0, 0, 192, 255),
            new RGBA8BitColor(0, 0, 195, 255),
            new RGBA8BitColor(0, 0, 200, 255),
            new RGBA8BitColor(0, 0, 204, 255),
            new RGBA8BitColor(0, 0, 208, 255),
            new RGBA8BitColor(0, 0, 211, 255),
            new RGBA8BitColor(0, 0, 216, 255),
            new RGBA8BitColor(0, 0, 220, 255),
            new RGBA8BitColor(0, 0, 224, 255),
            new RGBA8BitColor(0, 0, 227, 255),
            new RGBA8BitColor(0, 0, 232, 255),
            new RGBA8BitColor(0, 0, 236, 255),
            new RGBA8BitColor(0, 0, 240, 255),
            new RGBA8BitColor(0, 0, 243, 255),
            new RGBA8BitColor(0, 0, 248, 255),
            new RGBA8BitColor(0, 0, 252, 255),
            new RGBA8BitColor(0, 0, 255, 255),
            new RGBA8BitColor(0, 4, 255, 255),
            new RGBA8BitColor(0, 8, 255, 255),
            new RGBA8BitColor(0, 13, 255, 255),
            new RGBA8BitColor(0, 16, 255, 255),
            new RGBA8BitColor(0, 21, 255, 255),
            new RGBA8BitColor(0, 25, 255, 255),
            new RGBA8BitColor(0, 29, 255, 255),
            new RGBA8BitColor(0, 32, 255, 255),
            new RGBA8BitColor(0, 36, 255, 255),
            new RGBA8BitColor(0, 40, 255, 255),
            new RGBA8BitColor(0, 45, 255, 255),
            new RGBA8BitColor(0, 48, 255, 255),
            new RGBA8BitColor(0, 53, 255, 255),
            new RGBA8BitColor(0, 57, 255, 255),
            new RGBA8BitColor(0, 61, 255, 255),
            new RGBA8BitColor(0, 64, 255, 255),
            new RGBA8BitColor(0, 68, 255, 255),
            new RGBA8BitColor(0, 72, 255, 255),
            new RGBA8BitColor(0, 77, 255, 255),
            new RGBA8BitColor(0, 80, 255, 255),
            new RGBA8BitColor(0, 85, 255, 255),
            new RGBA8BitColor(0, 89, 255, 255),
            new RGBA8BitColor(0, 93, 255, 255),
            new RGBA8BitColor(0, 96, 255, 255),
            new RGBA8BitColor(0, 100, 255, 255),
            new RGBA8BitColor(0, 104, 255, 255),
            new RGBA8BitColor(0, 109, 255, 255),
            new RGBA8BitColor(0, 112, 255, 255),
            new RGBA8BitColor(0, 117, 255, 255),
            new RGBA8BitColor(0, 121, 255, 255),
            new RGBA8BitColor(0, 125, 255, 255),
            new RGBA8BitColor(0, 128, 255, 255),
            new RGBA8BitColor(0, 132, 255, 255),
            new RGBA8BitColor(0, 137, 255, 255),
            new RGBA8BitColor(0, 140, 255, 255),
            new RGBA8BitColor(0, 144, 255, 255),
            new RGBA8BitColor(0, 148, 255, 255),
            new RGBA8BitColor(0, 153, 255, 255),
            new RGBA8BitColor(0, 156, 255, 255),
            new RGBA8BitColor(0, 160, 255, 255),
            new RGBA8BitColor(0, 164, 255, 255),
            new RGBA8BitColor(0, 169, 255, 255),
            new RGBA8BitColor(0, 172, 255, 255),
            new RGBA8BitColor(0, 176, 255, 255),
            new RGBA8BitColor(0, 180, 255, 255),
            new RGBA8BitColor(0, 185, 255, 255),
            new RGBA8BitColor(0, 188, 255, 255),
            new RGBA8BitColor(0, 192, 255, 255),
            new RGBA8BitColor(0, 196, 255, 255),
            new RGBA8BitColor(0, 201, 255, 255),
            new RGBA8BitColor(0, 204, 255, 255),
            new RGBA8BitColor(0, 208, 255, 255),
            new RGBA8BitColor(0, 212, 255, 255),
            new RGBA8BitColor(0, 217, 255, 255),
            new RGBA8BitColor(0, 220, 255, 255),
            new RGBA8BitColor(0, 224, 255, 255),
            new RGBA8BitColor(0, 228, 255, 255),
            new RGBA8BitColor(0, 233, 255, 255),
            new RGBA8BitColor(0, 236, 255, 255),
            new RGBA8BitColor(0, 240, 255, 255),
            new RGBA8BitColor(0, 244, 255, 255),
            new RGBA8BitColor(0, 249, 255, 255),
            new RGBA8BitColor(0, 252, 255, 255),
            new RGBA8BitColor(1, 255, 254, 255),
            new RGBA8BitColor(5, 255, 250, 255),
            new RGBA8BitColor(10, 255, 245, 255),
            new RGBA8BitColor(14, 255, 242, 255),
            new RGBA8BitColor(17, 255, 238, 255),
            new RGBA8BitColor(21, 255, 234, 255),
            new RGBA8BitColor(26, 255, 229, 255),
            new RGBA8BitColor(30, 255, 226, 255),
            new RGBA8BitColor(33, 255, 222, 255),
            new RGBA8BitColor(37, 255, 218, 255),
            new RGBA8BitColor(42, 255, 213, 255),
            new RGBA8BitColor(46, 255, 210, 255),
            new RGBA8BitColor(49, 255, 206, 255),
            new RGBA8BitColor(53, 255, 202, 255),
            new RGBA8BitColor(58, 255, 197, 255),
            new RGBA8BitColor(62, 255, 194, 255),
            new RGBA8BitColor(66, 255, 190, 255),
            new RGBA8BitColor(69, 255, 186, 255),
            new RGBA8BitColor(74, 255, 181, 255),
            new RGBA8BitColor(78, 255, 178, 255),
            new RGBA8BitColor(82, 255, 174, 255),
            new RGBA8BitColor(85, 255, 170, 255),
            new RGBA8BitColor(90, 255, 165, 255),
            new RGBA8BitColor(94, 255, 162, 255),
            new RGBA8BitColor(98, 255, 158, 255),
            new RGBA8BitColor(101, 255, 154, 255),
            new RGBA8BitColor(106, 255, 149, 255),
            new RGBA8BitColor(110, 255, 146, 255),
            new RGBA8BitColor(114, 255, 142, 255),
            new RGBA8BitColor(117, 255, 138, 255),
            new RGBA8BitColor(122, 255, 133, 255),
            new RGBA8BitColor(126, 255, 130, 255),
            new RGBA8BitColor(130, 255, 126, 255),
            new RGBA8BitColor(133, 255, 122, 255),
            new RGBA8BitColor(137, 255, 118, 255),
            new RGBA8BitColor(141, 255, 114, 255),
            new RGBA8BitColor(146, 255, 109, 255),
            new RGBA8BitColor(150, 255, 105, 255),
            new RGBA8BitColor(154, 255, 101, 255),
            new RGBA8BitColor(158, 255, 98, 255),
            new RGBA8BitColor(162, 255, 94, 255),
            new RGBA8BitColor(165, 255, 90, 255),
            new RGBA8BitColor(169, 255, 86, 255),
            new RGBA8BitColor(173, 255, 82, 255),
            new RGBA8BitColor(178, 255, 77, 255),
            new RGBA8BitColor(182, 255, 73, 255),
            new RGBA8BitColor(186, 255, 69, 255),
            new RGBA8BitColor(190, 255, 66, 255),
            new RGBA8BitColor(194, 255, 62, 255),
            new RGBA8BitColor(197, 255, 58, 255),
            new RGBA8BitColor(201, 255, 54, 255),
            new RGBA8BitColor(205, 255, 50, 255),
            new RGBA8BitColor(210, 255, 45, 255),
            new RGBA8BitColor(214, 255, 41, 255),
            new RGBA8BitColor(218, 255, 37, 255),
            new RGBA8BitColor(222, 255, 33, 255),
            new RGBA8BitColor(226, 255, 30, 255),
            new RGBA8BitColor(229, 255, 26, 255),
            new RGBA8BitColor(233, 255, 22, 255),
            new RGBA8BitColor(237, 255, 18, 255),
            new RGBA8BitColor(242, 255, 13, 255),
            new RGBA8BitColor(246, 255, 9, 255),
            new RGBA8BitColor(250, 255, 5, 255),
            new RGBA8BitColor(254, 255, 1, 255),
            new RGBA8BitColor(255, 252, 0, 255),
            new RGBA8BitColor(255, 249, 0, 255),
            new RGBA8BitColor(255, 245, 0, 255),
            new RGBA8BitColor(255, 241, 0, 255),
            new RGBA8BitColor(255, 236, 0, 255),
            new RGBA8BitColor(255, 232, 0, 255),
            new RGBA8BitColor(255, 228, 0, 255),
            new RGBA8BitColor(255, 224, 0, 255),
            new RGBA8BitColor(255, 220, 0, 255),
            new RGBA8BitColor(255, 217, 0, 255),
            new RGBA8BitColor(255, 213, 0, 255),
            new RGBA8BitColor(255, 209, 0, 255),
            new RGBA8BitColor(255, 204, 0, 255),
            new RGBA8BitColor(255, 200, 0, 255),
            new RGBA8BitColor(255, 196, 0, 255),
            new RGBA8BitColor(255, 192, 0, 255),
            new RGBA8BitColor(255, 188, 0, 255),
            new RGBA8BitColor(255, 185, 0, 255),
            new RGBA8BitColor(255, 181, 0, 255),
            new RGBA8BitColor(255, 177, 0, 255),
            new RGBA8BitColor(255, 172, 0, 255),
            new RGBA8BitColor(255, 168, 0, 255),
            new RGBA8BitColor(255, 164, 0, 255),
            new RGBA8BitColor(255, 160, 0, 255),
            new RGBA8BitColor(255, 156, 0, 255),
            new RGBA8BitColor(255, 153, 0, 255),
            new RGBA8BitColor(255, 149, 0, 255),
            new RGBA8BitColor(255, 145, 0, 255),
            new RGBA8BitColor(255, 140, 0, 255),
            new RGBA8BitColor(255, 136, 0, 255),
            new RGBA8BitColor(255, 132, 0, 255),
            new RGBA8BitColor(255, 128, 0, 255),
            new RGBA8BitColor(255, 125, 0, 255),
            new RGBA8BitColor(255, 121, 0, 255),
            new RGBA8BitColor(255, 117, 0, 255),
            new RGBA8BitColor(255, 113, 0, 255),
            new RGBA8BitColor(255, 108, 0, 255),
            new RGBA8BitColor(255, 104, 0, 255),
            new RGBA8BitColor(255, 100, 0, 255),
            new RGBA8BitColor(255, 96, 0, 255),
            new RGBA8BitColor(255, 93, 0, 255),
            new RGBA8BitColor(255, 89, 0, 255),
            new RGBA8BitColor(255, 85, 0, 255),
            new RGBA8BitColor(255, 81, 0, 255),
            new RGBA8BitColor(255, 76, 0, 255),
            new RGBA8BitColor(255, 72, 0, 255),
            new RGBA8BitColor(255, 68, 0, 255),
            new RGBA8BitColor(255, 64, 0, 255),
            new RGBA8BitColor(255, 61, 0, 255),
            new RGBA8BitColor(255, 57, 0, 255),
            new RGBA8BitColor(255, 53, 0, 255),
            new RGBA8BitColor(255, 49, 0, 255),
            new RGBA8BitColor(255, 44, 0, 255),
            new RGBA8BitColor(255, 40, 0, 255),
            new RGBA8BitColor(255, 36, 0, 255),
            new RGBA8BitColor(255, 32, 0, 255),
            new RGBA8BitColor(255, 29, 0, 255),
            new RGBA8BitColor(255, 25, 0, 255),
            new RGBA8BitColor(255, 21, 0, 255),
            new RGBA8BitColor(255, 17, 0, 255),
            new RGBA8BitColor(255, 12, 0, 255),
            new RGBA8BitColor(255, 8, 0, 255),
            new RGBA8BitColor(255, 4, 0, 255),
            new RGBA8BitColor(255, 0, 0, 255),
            new RGBA8BitColor(252, 0, 0, 255),
            new RGBA8BitColor(248, 0, 0, 255),
            new RGBA8BitColor(244, 0, 0, 255),
            new RGBA8BitColor(240, 0, 0, 255),
            new RGBA8BitColor(235, 0, 0, 255),
            new RGBA8BitColor(231, 0, 0, 255),
            new RGBA8BitColor(227, 0, 0, 255),
            new RGBA8BitColor(224, 0, 0, 255),
            new RGBA8BitColor(220, 0, 0, 255),
            new RGBA8BitColor(216, 0, 0, 255),
            new RGBA8BitColor(212, 0, 0, 255),
            new RGBA8BitColor(208, 0, 0, 255),
            new RGBA8BitColor(203, 0, 0, 255),
            new RGBA8BitColor(199, 0, 0, 255),
            new RGBA8BitColor(195, 0, 0, 255),
            new RGBA8BitColor(192, 0, 0, 255),
            new RGBA8BitColor(188, 0, 0, 255),
            new RGBA8BitColor(184, 0, 0, 255),
            new RGBA8BitColor(180, 0, 0, 255),
            new RGBA8BitColor(176, 0, 0, 255),
            new RGBA8BitColor(171, 0, 0, 255),
            new RGBA8BitColor(167, 0, 0, 255),
            new RGBA8BitColor(163, 0, 0, 255),
            new RGBA8BitColor(160, 0, 0, 255),
            new RGBA8BitColor(156, 0, 0, 255),
            new RGBA8BitColor(152, 0, 0, 255),
            new RGBA8BitColor(148, 0, 0, 255),
            new RGBA8BitColor(144, 0, 0, 255),
            new RGBA8BitColor(139, 0, 0, 255),
            new RGBA8BitColor(135, 0, 0, 255),
            new RGBA8BitColor(132, 0, 0, 255),
            new RGBA8BitColor(128, 0, 0, 255),
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
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        public byte R { get; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        public byte G { get; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        public byte B { get; }

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        public byte A { get; }

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
                BlendChannel(from.R, to.R, amount, inverseAmount),
                BlendChannel(from.G, to.G, amount, inverseAmount),
                BlendChannel(from.B, to.B, amount, inverseAmount),
                BlendChannel(from.A, to.A, amount, inverseAmount));
        }

        /// <summary>
        /// Indicates whether this color has the same channel values as another color.
        /// </summary>
        /// <param name="other">The color to compare with this color.</param>
        /// <returns><see langword="true"/> if all channel values are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RGBA8BitColor other) =>
            R == other.R &&
            G == other.G &&
            B == other.B &&
            A == other.A;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RGBA8BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "rgba8({0}, {1}, {2}, {3})", R, G, B, A);

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

