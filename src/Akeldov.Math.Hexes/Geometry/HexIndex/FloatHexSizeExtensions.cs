using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Performs the ConvertHexApothemToRadius operation.
        /// </summary>
        /// <param name="apothem">The apothem value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertHexApothemToRadius(this float apothem)
        {
            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            return Constants.Apothem2Radius * apothem;
        }

        /// <summary>
        /// Performs the ConvertHexRadiusToApothem operation.
        /// </summary>
        /// <param name="radius">The radius value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertHexRadiusToApothem(this float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            return Constants.Radius2Apothem * radius;
        }
    }
}
