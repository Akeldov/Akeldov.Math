namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides tolerance constants used by two-dimensional geometry operations.
    /// </summary>
    public static class GeometryConstants
    {
        /// <summary>
        /// The default absolute tolerance for geometry comparisons.
        /// </summary>
        public const float GeometryEpsilon = 1e-6f;

        /// <summary>
        /// The squared default absolute tolerance for squared-distance comparisons.
        /// </summary>
        public const float GeometryEpsilonSquared = GeometryEpsilon * GeometryEpsilon;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static void ValidateGeometryEpsilon(float geometryEpsilon, string parameterName)
        {
            if (geometryEpsilon < 0f || float.IsNaN(geometryEpsilon) || float.IsInfinity(geometryEpsilon))
                ThrowInvalidGeometryEpsilon(parameterName);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static void ValidateFiniteAngle(float angle, string parameterName)
        {
            if (float.IsNaN(angle) || float.IsInfinity(angle))
                ThrowInvalidAngle(parameterName);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void ThrowInvalidGeometryEpsilon(string parameterName)
        {
            throw new System.ArgumentOutOfRangeException(parameterName, "Geometry epsilon must be finite and non-negative.");
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void ThrowInvalidAngle(string parameterName)
        {
            throw new System.ArgumentOutOfRangeException(parameterName, "Angle must be finite.");
        }
    }
}
