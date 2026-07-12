using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines how QRS index labels are rendered inside topology cells.
    /// </summary>
    public readonly struct HexMapTopologyQRSLabelsRasterizationOptions
    {
        /// <summary>
        /// Initializes QRS label rasterization options.
        /// </summary>
        public HexMapTopologyQRSLabelsRasterizationOptions(
            TrueTypeFont font,
            float fontSize,
            Gray8BitColor color,
            float edgeFalloff,
            VectorXY offset = default)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            if (fontSize <= 0f || float.IsNaN(fontSize) || float.IsInfinity(fontSize))
                throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be finite and positive.");
            if (edgeFalloff < 0f || float.IsNaN(edgeFalloff) || float.IsInfinity(edgeFalloff))
                throw new ArgumentOutOfRangeException(nameof(edgeFalloff), "Edge falloff must be finite and non-negative.");
            if (!offset.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(offset), "Label offset components must be finite.");

            FontSize = fontSize;
            Color = color;
            EdgeFalloff = edgeFalloff;
            Offset = offset;
        }

        /// <summary>Gets the TrueType font used for labels.</summary>
        public TrueTypeFont Font { get; }
        /// <summary>Gets the label font size in coordinate-space units.</summary>
        public float FontSize { get; }
        /// <summary>Gets the grayscale label color.</summary>
        public Gray8BitColor Color { get; }
        /// <summary>Gets the label edge falloff in coordinate-space units.</summary>
        public float EdgeFalloff { get; }
        /// <summary>Gets the label offset from the hex center in coordinate-space units.</summary>
        public VectorXY Offset { get; }
    }
}
