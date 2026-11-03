namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Specifies how an sRGB color is darkened or lightened while preserving alpha.</summary>
    public enum ColorAdjustmentMode
    {
        /// <summary>Interpolates encoded sRGB channels towards black or white without gamma correction.</summary>
        Srgb = 0,

        /// <summary>Decodes sRGB, interpolates linear RGB channels towards black or white, then encodes sRGB.</summary>
        LinearRgb = 1,

        /// <summary>Moves OKLCH lightness towards zero or one, retaining hue and chroma where possible.</summary>
        /// <remarks>Out-of-gamut colors have their chroma reduced at fixed lightness and hue to fit sRGB.
        /// Alpha is unchanged. Hue is undefined for achromatic colors.</remarks>
        Oklch = 2
    }
}
