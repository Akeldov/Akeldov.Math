using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a two-dimensional vector with integer components.
    /// </summary>
    public readonly struct VectorXYInt : IEquatable<VectorXYInt>
    {
        /// <summary>
        /// Initializes a new integer vector with the specified components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public VectorXYInt(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the X component.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y component.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the Euclidean length of this vector.
        /// </summary>
        public float Length => MathF.Sqrt((float)X * X + (float)Y * Y);

        /// <summary>
        /// Gets the vector with both components equal to zero.
        /// </summary>
        public static VectorXYInt Zero => new VectorXYInt(0, 0);

        /// <summary>
        /// Gets the vector with both components equal to one.
        /// </summary>
        public static VectorXYInt One => new VectorXYInt(1, 1);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorXYInt other && Equals(other);

        /// <summary>
        /// Indicates whether this vector has the same components as another vector.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> if both components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorXYInt other) => X == other.X && Y == other.Y;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", X, Y);

        /// <summary>
        /// Deconstructs this vector into its X and Y components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Indicates whether two integer vectors have equal components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if both components are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(VectorXYInt left, VectorXYInt right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two integer vectors have different components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if any component differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(VectorXYInt left, VectorXYInt right) => !left.Equals(right);

        /// <summary>
        /// Adds two integer vectors component by component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The component-wise sum.</returns>
        public static VectorXYInt operator +(VectorXYInt left, VectorXYInt right) =>
            new VectorXYInt(left.X + right.X, left.Y + right.Y);

        /// <summary>
        /// Subtracts one integer vector from another component by component.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        /// <returns>The component-wise difference.</returns>
        public static VectorXYInt operator -(VectorXYInt left, VectorXYInt right) =>
            new VectorXYInt(left.X - right.X, left.Y - right.Y);

        /// <summary>
        /// Multiplies an integer vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYInt operator *(VectorXYInt vector, int scalar) =>
            new VectorXYInt(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies an integer vector by an integer scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYInt operator *(int scalar, VectorXYInt vector) =>
            new VectorXYInt(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies an integer vector by a floating-point scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(VectorXYInt vector, float scalar) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Multiplies an integer vector by a floating-point scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXY operator *(float scalar, VectorXYInt vector) =>
            new VectorXY(vector.X * scalar, vector.Y * scalar);

        /// <summary>
        /// Divides an integer vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar divisor.</param>
        /// <returns>The component-wise integer quotient.</returns>
        /// <exception cref="DivideByZeroException">Thrown when <paramref name="scalar"/> is zero.</exception>
        public static VectorXYInt operator /(VectorXYInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");
            return new VectorXYInt(vector.X / scalar, vector.Y / scalar);
        }
    }
}
