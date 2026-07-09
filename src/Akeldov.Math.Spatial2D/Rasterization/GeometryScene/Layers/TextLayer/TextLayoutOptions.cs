namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Describes simple text layout options for geometry scene text layers.
    /// </summary>
    public sealed class TextLayoutOptions
    {
        /// <summary>
        /// Gets or sets the text anchor used to position laid-out text relative to the origin.
        /// </summary>
        public TextAnchor Anchor { get; set; } = TextAnchor.BaselineLeft;

        /// <summary>
        /// Gets or sets the additional spacing between adjacent glyph advances, in world coordinate units.
        /// </summary>
        public float LetterSpacing { get; set; }

        /// <summary>
        /// Gets or sets the additional spacing between line advances, in world coordinate units.
        /// </summary>
        public float LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether legacy TrueType kerning pairs should be applied when available.
        /// </summary>
        public bool UseKerning { get; set; } = true;
    }
}
