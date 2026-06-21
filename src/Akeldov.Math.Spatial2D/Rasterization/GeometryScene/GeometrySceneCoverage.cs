namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class GeometrySceneCoverage
    {
        public static float GetOutsideCoverage(float distance, float radius, float edgeFalloff)
        {
            if (distance <= radius)
                return 1f;

            if (edgeFalloff <= 0f)
                return 0f;

            return 1f - ClampNormalized((distance - radius) / edgeFalloff);
        }

        public static float GetFillCoverage(float signedDistance, float edgeFalloff)
        {
            if (signedDistance <= 0f)
                return 1f;

            if (edgeFalloff <= 0f)
                return 0f;

            return 1f - ClampNormalized(signedDistance / edgeFalloff);
        }

        private static float ClampNormalized(float value)
        {
            if (value <= 0f)
                return 0f;

            if (value >= 1f)
                return 1f;

            return value;
        }
    }
}
