using System;

namespace Akeldov.Math.Spatial2D.Fields
{
    /// <summary>
    /// Represents integer fields that provide pointwise minimum and maximum bounds.
    /// </summary>
    public readonly struct IntFieldRange
    {
        /// <summary>
        /// Initializes an integer field range.
        /// </summary>
        /// <param name="minField">The field that provides the pointwise minimum bound.</param>
        /// <param name="maxField">The field that provides the pointwise maximum bound.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="minField"/> or <paramref name="maxField"/> is
        /// <see langword="null"/>.
        /// </exception>
        public IntFieldRange(IIntField minField, IIntField maxField)
        {
            MinField = minField ?? throw new ArgumentNullException(nameof(minField));
            MaxField = maxField ?? throw new ArgumentNullException(nameof(maxField));
        }

        /// <summary>
        /// Gets the field that provides the pointwise minimum bound.
        /// </summary>
        public IIntField MinField { get; }

        /// <summary>
        /// Gets the field that provides the pointwise maximum bound.
        /// </summary>
        public IIntField MaxField { get; }

        /// <summary>
        /// Deconstructs the range into its minimum and maximum fields.
        /// </summary>
        /// <param name="minField">Receives the field that provides the pointwise minimum bound.</param>
        /// <param name="maxField">Receives the field that provides the pointwise maximum bound.</param>
        public void Deconstruct(out IIntField minField, out IIntField maxField)
        {
            minField = MinField;
            maxField = MaxField;
        }
    }
}
