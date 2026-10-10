using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TrueTypeGlyphOutline
    {
        public static readonly TrueTypeGlyphOutline Empty = new TrueTypeGlyphOutline(Array.Empty<TrueTypeGlyphContour>());

        public TrueTypeGlyphOutline(IReadOnlyList<TrueTypeGlyphContour> contours)
        {
            Contours = contours ?? throw new ArgumentNullException(nameof(contours));
        }

        public IReadOnlyList<TrueTypeGlyphContour> Contours { get; }

        public TrueTypeGlyphOutline Transform(float a, float b, float c, float d, float dx, float dy)
        {
            var contours = new TrueTypeGlyphContour[Contours.Count];
            for (int i = 0; i < contours.Length; i++)
            {
                contours[i] = Contours[i].Transform(a, b, c, d, dx, dy);
            }

            return new TrueTypeGlyphOutline(contours);
        }
    }
}
