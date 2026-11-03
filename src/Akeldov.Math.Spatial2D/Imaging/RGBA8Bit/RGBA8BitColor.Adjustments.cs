using System;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public readonly partial struct RGBA8BitColor
    {
        /// <summary>Darkens this color by interpolating encoded sRGB channels towards black.</summary>
        /// <param name="amount">The finite adjustment amount in [0, 1]. Zero returns this color; one returns black with the same alpha.</param>
        /// <returns>A new color with adjusted RGB channels and the original alpha.</returns>
        /// <remarks>Equivalent to <see cref="Darken(float, ColorAdjustmentMode)"/> with <see cref="ColorAdjustmentMode.Srgb"/>.
        /// The input and output RGB channels are interpreted as sRGB. This does not scale physical luminance.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The amount is not finite or is outside [0, 1].</exception>
        public RGBA8BitColor Darken(float amount) => Darken(amount, ColorAdjustmentMode.Srgb);

        /// <summary>Darkens this sRGB color using the selected adjustment mode.</summary>
        /// <param name="amount">The finite adjustment amount in [0, 1]. Zero returns this color; one returns black with the same alpha.</param>
        /// <param name="mode">The adjustment algorithm. See <see cref="ColorAdjustmentMode"/> for each mode's behavior.</param>
        /// <returns>A new sRGB color with the original alpha.</returns>
        /// <remarks>Scales encoded sRGB channels, linear RGB channels, or OKLCH lightness by one minus the amount.
        /// In OKLCH, hue is retained and chroma is reduced only when needed to fit sRGB.
        /// Equal amounts in different modes need not produce equal changes in luminance or perceived lightness.
        /// Channels are rounded to the nearest byte, with midpoint ties rounded to even.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The amount is not finite or is outside [0, 1], or the mode is unsupported.</exception>
        public RGBA8BitColor Darken(float amount, ColorAdjustmentMode mode)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f || amount > 1f)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (mode != ColorAdjustmentMode.Srgb && mode != ColorAdjustmentMode.LinearRgb && mode != ColorAdjustmentMode.Oklch)
                throw new ArgumentOutOfRangeException(nameof(mode));
            return AdjustLightness(amount, mode, lighten: false);
        }

        /// <summary>Lightens this color by interpolating encoded sRGB channels towards white.</summary>
        /// <param name="amount">The finite adjustment amount in [0, 1]. Zero returns this color; one returns white with the same alpha.</param>
        /// <returns>A new color with adjusted RGB channels and the original alpha.</returns>
        /// <remarks>Equivalent to <see cref="Lighten(float, ColorAdjustmentMode)"/> with <see cref="ColorAdjustmentMode.Srgb"/>.
        /// The input and output RGB channels are interpreted as sRGB. This does not interpolate physical luminance.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The amount is not finite or is outside [0, 1].</exception>
        public RGBA8BitColor Lighten(float amount) => Lighten(amount, ColorAdjustmentMode.Srgb);

        /// <summary>Lightens this sRGB color using the selected adjustment mode.</summary>
        /// <param name="amount">The finite adjustment amount in [0, 1]. Zero returns this color; one returns white with the same alpha.</param>
        /// <param name="mode">The adjustment algorithm. See <see cref="ColorAdjustmentMode"/> for each mode's behavior.</param>
        /// <returns>A new sRGB color with the original alpha.</returns>
        /// <remarks>Moves encoded sRGB channels, linear RGB channels, or OKLCH lightness towards one by the given amount.
        /// In OKLCH, hue is retained and chroma is reduced only when needed to fit sRGB.
        /// Equal amounts in different modes need not produce equal changes in luminance or perceived lightness.
        /// Channels are rounded to the nearest byte, with midpoint ties rounded to even.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">The amount is not finite or is outside [0, 1], or the mode is unsupported.</exception>
        public RGBA8BitColor Lighten(float amount, ColorAdjustmentMode mode)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f || amount > 1f)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (mode != ColorAdjustmentMode.Srgb && mode != ColorAdjustmentMode.LinearRgb && mode != ColorAdjustmentMode.Oklch)
                throw new ArgumentOutOfRangeException(nameof(mode));
            return AdjustLightness(amount, mode, lighten: true);
        }

        private RGBA8BitColor AdjustLightness(float amount, ColorAdjustmentMode mode, bool lighten)
        {
            if (amount == 0f)
                return this;
            if (amount == 1f)
            {
                byte target = lighten ? byte.MaxValue : (byte)0;
                return new RGBA8BitColor(target, target, target, A);
            }
            float Adjust(float channel) => lighten ? channel + (1f - channel) * amount : channel * (1f - amount);
            float r = R / 255f;
            float g = G / 255f;
            float b = B / 255f;
            if (mode == ColorAdjustmentMode.Srgb)
                return new RGBA8BitColor(ToAdjustedChannel(Adjust(r)), ToAdjustedChannel(Adjust(g)), ToAdjustedChannel(Adjust(b)), A);

            r = DecodeSrgb(r);
            g = DecodeSrgb(g);
            b = DecodeSrgb(b);
            if (mode == ColorAdjustmentMode.LinearRgb)
            {
                r = Adjust(r);
                g = Adjust(g);
                b = Adjust(b);
            }
            else
            {
                if (R == G && G == B)
                {
                    // Neutral OKLab has L = cbrt(linear RGB), a = b = 0.
                    // Avoid matrix roundoff introducing a tint at byte-rounding boundaries.
                    float neutralLightness = Adjust(MathF.Cbrt(r));
                    byte neutral = ToAdjustedChannel(EncodeSrgb(neutralLightness * neutralLightness * neutralLightness));
                    return new RGBA8BitColor(neutral, neutral, neutral, A);
                }
                // Public-domain conversion matrices by Bjorn Ottosson:
                // https://bottosson.github.io/posts/oklab/
                float l = MathF.Cbrt(0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b);
                float m = MathF.Cbrt(0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b);
                float s = MathF.Cbrt(0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b);
                float lightness = Adjust(0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s);
                float a = 1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s;
                float labB = 0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s;
                (r, g, b) = MapOklabToLinearSrgb(lightness, a, labB);
            }
            return new RGBA8BitColor(ToAdjustedChannel(EncodeSrgb(r)), ToAdjustedChannel(EncodeSrgb(g)),
                ToAdjustedChannel(EncodeSrgb(b)), A);
        }

        private static (float R, float G, float B) MapOklabToLinearSrgb(float lightness, float a, float b)
        {
            var rgb = OklabToLinear(lightness, a, b);
            if (IsInSrgbGamut(rgb.R, rgb.G, rgb.B))
                return rgb;

            // Scaling a and b equally reduces OKLCH chroma without changing hue.
            // Search from neutral (in gamut) to the requested chroma.
            float low = 0f;
            float high = 1f;
            for (int i = 0; i < 24; i++)
            {
                float scale = (low + high) * 0.5f;
                var candidate = OklabToLinear(lightness, a * scale, b * scale);
                if (IsInSrgbGamut(candidate.R, candidate.G, candidate.B))
                    low = scale;
                else
                    high = scale;
            }
            return OklabToLinear(lightness, a * low, b * low);
        }

        private static (float R, float G, float B) OklabToLinear(float lightness, float a, float b)
        {
            float l = lightness + 0.3963377774f * a + 0.2158037573f * b;
            float m = lightness - 0.1055613458f * a - 0.0638541728f * b;
            float s = lightness - 0.0894841775f * a - 1.2914855480f * b;
            l *= l * l;
            m *= m * m;
            s *= s * s;
            return (4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s,
                -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s,
                -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s);
        }

        private static bool IsInSrgbGamut(float r, float g, float b) =>
            r >= 0f && r <= 1f && g >= 0f && g <= 1f && b >= 0f && b <= 1f;

        private static float DecodeSrgb(float channel) => channel <= 0.04045f
            ? channel / 12.92f : MathF.Pow((channel + 0.055f) / 1.055f, 2.4f);

        private static float EncodeSrgb(float channel) => channel <= 0.0031308f
            ? channel * 12.92f : 1.055f * MathF.Pow(channel, 1f / 2.4f) - 0.055f;

        // Clamp only conversion roundoff; out-of-gamut OKLCH colors are mapped by reducing chroma above.
        private static byte ToAdjustedChannel(float channel) =>
            (byte)MathF.Round(MathF.Min(1f, MathF.Max(0f, channel)) * 255f);
    }
}
