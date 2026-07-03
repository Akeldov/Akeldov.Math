using System.Runtime.CompilerServices;
using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="fractionalPoint">The fractionalPoint value.</param>
        /// <param name="hexRadius">The hexRadius value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToNormalizedAxial(this VectorQRS fractionalPoint, float hexRadius)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return fractionalPoint / hexRadius;
        }
    }
}
