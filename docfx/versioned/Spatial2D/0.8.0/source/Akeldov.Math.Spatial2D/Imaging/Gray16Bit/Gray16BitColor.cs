using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Represents a 16-bit grayscale color.
    /// </summary>
    public readonly struct Gray16BitColor : IEquatable<Gray16BitColor>
    {
        /// <summary>Initializes a new 16-bit grayscale color.</summary>
        /// <param name="value">The grayscale intensity value.</param>
        public Gray16BitColor(ushort value) => Value = value;

        /// <summary>Gets the grayscale intensity value.</summary>
        public ushort Value { get; }

        /// <summary>Gets black.</summary>
        public static Gray16BitColor Black => new Gray16BitColor(ushort.MinValue);

        /// <summary>Gets white.</summary>
        public static Gray16BitColor White => new Gray16BitColor(ushort.MaxValue);

        /// <summary>Creates a 16-bit grayscale color from a normalized intensity.</summary>
        /// <param name="value">The normalized intensity. Values outside the 0 to 1 range are clamped.</param>
        /// <returns>A 16-bit grayscale color.</returns>
        public static Gray16BitColor FromNormalized(float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Normalized intensity must not be NaN.");

            value = MathF.Min(MathF.Max(value, 0f), 1f);
            return new Gray16BitColor((ushort)MathF.Round(value * ushort.MaxValue));
        }

        /// <summary>Linearly blends two 16-bit grayscale colors.</summary>
        public static Gray16BitColor Blend(Gray16BitColor from, Gray16BitColor to, float amount)
        {
            if (float.IsNaN(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Blend amount must not be NaN.");

            amount = MathF.Min(MathF.Max(amount, 0f), 1f);
            return new Gray16BitColor((ushort)MathF.Round(from.Value * (1f - amount) + to.Value * amount));
        }

        /// <inheritdoc/>
        public bool Equals(Gray16BitColor other) => Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Gray16BitColor other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "gray16({0})", Value);

        /// <summary>Indicates whether two colors are equal.</summary>
        public static bool operator ==(Gray16BitColor left, Gray16BitColor right) => left.Equals(right);

        /// <summary>Indicates whether two colors are different.</summary>
        public static bool operator !=(Gray16BitColor left, Gray16BitColor right) => !left.Equals(right);
    }
}
