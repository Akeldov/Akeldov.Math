using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Multiplies two integer vectors component by component.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The component-wise product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt HadamardMultiply(this VectorXYInt a, VectorXYInt b)
        {
            return new VectorXYInt(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Multiplies an integer vector and a floating-point vector component by component.
        /// </summary>
        /// <param name="a">The integer vector.</param>
        /// <param name="b">The floating-point vector.</param>
        /// <returns>The component-wise product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardMultiply(this VectorXYInt a, VectorXY b)
        {
            return new VectorXY(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Divides an integer vector by a floating-point vector component by component.
        /// </summary>
        /// <param name="a">The dividend vector.</param>
        /// <param name="b">The divisor vector.</param>
        /// <returns>The component-wise quotient.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardDivide(this VectorXYInt a, VectorXY b)
        {
            return new VectorXY(a.X / b.X, a.Y / b.Y);
        }

        /// <summary>
        /// Divides two integer vectors component by component and returns floating-point quotients.
        /// </summary>
        /// <param name="a">The dividend vector.</param>
        /// <param name="b">The divisor vector.</param>
        /// <returns>The component-wise quotient.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardDivide(this VectorXYInt a, VectorXYInt b)
        {
            return new VectorXY((float)a.X / b.X, (float)a.Y / b.Y);
        }
    }
}
