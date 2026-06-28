using System;
using System.Drawing;
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
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        public ushort R { get; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        public ushort G { get; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        public ushort B { get; }

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        public ushort A { get; }

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
        /// Creates a 16-bit RGBA temperature-style color from a normalized value.
        /// </summary>
        /// <param name="normalizedValue">The normalized value. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>
        /// A 16-bit RGBA color interpolated from the same blue-cyan-green-yellow-red temperature palette as
        /// <see cref="RGBA8BitColor.FromTemperature(float)"/>.
        /// </returns>
        public static RGBA16BitColor FromTemperature(float normalizedValue)
        {
            if (float.IsNaN(normalizedValue) || float.IsInfinity(normalizedValue))
                throw new ArgumentOutOfRangeException(nameof(normalizedValue), "Normalized temperature value must be finite.");

            normalizedValue = MathF.Min(MathF.Max(normalizedValue, 0f), 1f);
            float scaled = normalizedValue * (RGBA8BitColor.TemperaturePalette.Length - 1);
            int index = (int)MathF.Floor(scaled);

            if (index >= RGBA8BitColor.TemperaturePalette.Length - 1)
                return FromRGBA8BitColor(RGBA8BitColor.TemperaturePalette[RGBA8BitColor.TemperaturePalette.Length - 1]);

            float amount = scaled - index;
            return Blend(
                FromRGBA8BitColor(RGBA8BitColor.TemperaturePalette[index]),
                FromRGBA8BitColor(RGBA8BitColor.TemperaturePalette[index + 1]),
                amount);
        }

        /// <summary>
        /// Creates a 16-bit RGBA temperature-style color from a value and its value range.
        /// </summary>
        /// <param name="value">The value to map to the temperature color scale.</param>
        /// <param name="min">The range minimum value.</param>
        /// <param name="max">The range maximum value.</param>
        /// <returns>
        /// A 16-bit RGBA color on the blue-cyan-green-yellow-red temperature scale, where lower values are blue
        /// and higher values are red, interpolated at 16-bit channel precision.
        /// </returns>
        public static RGBA16BitColor FromTemperature(float value, float min, float max)
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
        /// Returns a color with the same RGB channels and alpha multiplied by <paramref name="coverage"/>.
        /// </summary>
        /// <param name="coverage">The alpha coverage multiplier.</param>
        /// <returns>A color with scaled alpha.</returns>
        public RGBA16BitColor ScaleAlpha(float coverage)
        {
            if (coverage <= 0f || A == 0)
                return default(RGBA16BitColor);

            if (coverage >= 1f)
                return this;

            var newNormalizedAlpha = A * coverage;
            ushort newAlpha = 0;
            if (newNormalizedAlpha <= 0f)
                newAlpha = 0;

            if (newNormalizedAlpha >= ushort.MaxValue)
                newAlpha = ushort.MaxValue;

            newAlpha = (ushort)MathF.Round(newNormalizedAlpha);

            return new RGBA16BitColor(
                R,
                G,
                B,
                newAlpha);
        }

        /// <summary>
        /// Composites a foreground 16-bit RGBA color over a background color.
        /// </summary>
        /// <param name="background">The background color.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <returns>The alpha-composited color.</returns>
        public static RGBA16BitColor AlphaOver(RGBA16BitColor background, RGBA16BitColor foreground)
        {
            float foregroundAlpha = foreground.A / (float)ushort.MaxValue;
            float backgroundAlpha = background.A / (float)ushort.MaxValue;
            float outputAlpha = foregroundAlpha + backgroundAlpha * (1f - foregroundAlpha);

            if (outputAlpha <= 0f)
                return default(RGBA16BitColor);

            float backgroundAmount = backgroundAlpha * (1f - foregroundAlpha);

            return new RGBA16BitColor(
                ToChannel((foreground.R * foregroundAlpha + background.R * backgroundAmount) / outputAlpha),
                ToChannel((foreground.G * foregroundAlpha + background.G * backgroundAmount) / outputAlpha),
                ToChannel((foreground.B * foregroundAlpha + background.B * backgroundAmount) / outputAlpha),
                ToChannel(outputAlpha * ushort.MaxValue));
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
        public bool Equals(RGBA16BitColor other) =>
            R == other.R &&
            G == other.G &&
            B == other.B &&
            A == other.A;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RGBA16BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "rgba16({0}, {1}, {2}, {3})", R, G, B, A);

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

        private static RGBA16BitColor FromRGBA8BitColor(RGBA8BitColor color)
        {
            return new RGBA16BitColor(
                To16BitChannel(color.R),
                To16BitChannel(color.G),
                To16BitChannel(color.B),
                To16BitChannel(color.A));
        }

        private static ushort ToChannel(float value)
        {
            if (value <= 0f)
                return 0;

            if (value >= ushort.MaxValue)
                return ushort.MaxValue;

            return (ushort)MathF.Round(value);
        }

        private static ushort To16BitChannel(byte channel) => (ushort)(channel * 257);
    }
}
