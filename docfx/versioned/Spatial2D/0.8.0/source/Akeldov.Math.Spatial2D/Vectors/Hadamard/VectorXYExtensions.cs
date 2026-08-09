using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Multiplies two vectors component by component.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The component-wise product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardMultiply(this VectorXY a, VectorXY b)
        {
            return new VectorXY(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Multiplies a floating-point vector and an integer vector component by component.
        /// </summary>
        /// <param name="a">The floating-point vector.</param>
        /// <param name="b">The integer vector.</param>
        /// <returns>The component-wise product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardMultiply(this VectorXY a, VectorXYInt b)
        {
            return new VectorXY(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Divides two vectors component by component.
        /// </summary>
        /// <param name="a">The dividend vector.</param>
        /// <param name="b">The divisor vector.</param>
        /// <returns>The component-wise quotient.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardDivide(this VectorXY a, VectorXY b)
        {
            return new VectorXY(a.X / b.X, a.Y / b.Y);
        }

        /// <summary>
        /// Divides a floating-point vector by an integer vector component by component.
        /// </summary>
        /// <param name="a">The dividend vector.</param>
        /// <param name="b">The divisor vector.</param>
        /// <returns>The component-wise quotient.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY HadamardDivide(this VectorXY a, VectorXYInt b)
        {
            return new VectorXY(a.X / b.X, a.Y / b.Y);
        }
    }
}
