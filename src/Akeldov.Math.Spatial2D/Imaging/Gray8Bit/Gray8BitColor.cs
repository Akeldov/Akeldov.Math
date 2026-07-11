using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Represents an 8-bit grayscale color.
    /// </summary>
    public readonly struct Gray8BitColor : IEquatable<Gray8BitColor>
    {
        /// <summary>Initializes a new 8-bit grayscale color.</summary>
        /// <param name="value">The grayscale intensity value.</param>
        public Gray8BitColor(byte value) => Value = value;

        /// <summary>Gets the grayscale intensity value.</summary>
        public byte Value { get; }

        /// <summary>Gets black.</summary>
        public static Gray8BitColor Black => new Gray8BitColor(byte.MinValue);

        /// <summary>Gets white.</summary>
        public static Gray8BitColor White => new Gray8BitColor(byte.MaxValue);

        /// <summary>Creates an 8-bit grayscale color from a normalized intensity.</summary>
        /// <param name="value">The normalized intensity. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>An 8-bit grayscale color.</returns>
        public static Gray8BitColor FromNormalized(float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Normalized intensity must not be NaN.");

            value = MathF.Min(MathF.Max(value, 0f), 1f);
            return new Gray8BitColor((byte)MathF.Round(value * byte.MaxValue));
        }

        /// <summary>Linearly blends two 8-bit grayscale colors.</summary>
        public static Gray8BitColor Blend(Gray8BitColor from, Gray8BitColor to, float amount)
        {
            if (float.IsNaN(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Blend amount must not be NaN.");

            amount = MathF.Min(MathF.Max(amount, 0f), 1f);
            return new Gray8BitColor((byte)MathF.Round(from.Value * (1f - amount) + to.Value * amount));
        }

        /// <inheritdoc/>
        public bool Equals(Gray8BitColor other) => Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Gray8BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "gray8({0})", Value);

        /// <summary>Indicates whether two colors are equal.</summary>
        public static bool operator ==(Gray8BitColor left, Gray8BitColor right) => left.Equals(right);

        /// <summary>Indicates whether two colors are different.</summary>
        public static bool operator !=(Gray8BitColor left, Gray8BitColor right) => !left.Equals(right);
    }
}
