using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides conversions between the radius and apothem of a regular hexagon.
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Converts a hex apothem to the corresponding center-to-vertex radius.
        /// </summary>
        /// <param name="apothem">The positive center-to-edge distance in any coordinate-space unit.</param>
        /// <returns>The radius in the same coordinate-space unit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertHexApothemToRadius(this float apothem)
        {
            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            return Constants.Apothem2Radius * apothem;
        }

        /// <summary>
        /// Converts a center-to-vertex hex radius to the corresponding apothem.
        /// </summary>
        /// <param name="radius">The positive center-to-vertex distance in any coordinate-space unit.</param>
        /// <returns>The apothem in the same coordinate-space unit.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertHexRadiusToApothem(this float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            return Constants.Radius2Apothem * radius;
        }
    }
}
