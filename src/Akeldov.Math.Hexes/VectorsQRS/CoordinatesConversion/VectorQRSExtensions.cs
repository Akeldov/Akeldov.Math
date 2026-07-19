using System.Runtime.CompilerServices;
using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides scale conversions for fractional QRS coordinates.
    /// </summary>
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Converts radius-scaled QRS coordinates to normalized axial coordinates.
        /// </summary>
        /// <param name="fractionalPoint">The QRS coordinates expressed in coordinate-space units.</param>
        /// <param name="hexRadius">The positive hex radius in the same coordinate-space unit.</param>
        /// <returns>The QRS coordinates measured in hex-radius units.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToNormalizedAxial(this VectorQRS fractionalPoint, float hexRadius)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return fractionalPoint / hexRadius;
        }
    }
}
