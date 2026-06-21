using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class GeometrySceneColor
    {
        public static RGBA16BitColor AlphaOver(RGBA16BitColor background, RGBA16BitColor foreground)
        {
            float foregroundAlpha = foreground.Alpha / (float)ushort.MaxValue;
            float backgroundAlpha = background.Alpha / (float)ushort.MaxValue;
            float outputAlpha = foregroundAlpha + backgroundAlpha * (1f - foregroundAlpha);

            if (outputAlpha <= 0f)
                return default(RGBA16BitColor);

            float backgroundAmount = backgroundAlpha * (1f - foregroundAlpha);

            return new RGBA16BitColor(
                ToChannel((foreground.Red * foregroundAlpha + background.Red * backgroundAmount) / outputAlpha),
                ToChannel((foreground.Green * foregroundAlpha + background.Green * backgroundAmount) / outputAlpha),
                ToChannel((foreground.Blue * foregroundAlpha + background.Blue * backgroundAmount) / outputAlpha),
                ToChannel(outputAlpha * ushort.MaxValue));
        }

        public static RGBA16BitColor WithAlphaCoverage(RGBA16BitColor color, float coverage)
        {
            if (coverage <= 0f || color.Alpha == 0)
                return default(RGBA16BitColor);

            if (coverage >= 1f)
                return color;

            return new RGBA16BitColor(
                color.Red,
                color.Green,
                color.Blue,
                ToChannel(color.Alpha * coverage));
        }

        private static ushort ToChannel(float value)
        {
            if (value <= 0f)
                return 0;

            if (value >= ushort.MaxValue)
                return ushort.MaxValue;

            return (ushort)MathF.Round(value);
        }
    }
}
