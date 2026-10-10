using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TrueTypeGlyphContour
    {
        public TrueTypeGlyphContour(IReadOnlyList<TrueTypeGlyphSegment> segments)
        {
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public IReadOnlyList<TrueTypeGlyphSegment> Segments { get; }

        public TrueTypeGlyphContour Transform(float a, float b, float c, float d, float dx, float dy)
        {
            var segments = new TrueTypeGlyphSegment[Segments.Count];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = Segments[i].Transform(a, b, c, d, dx, dy);
            }

            return new TrueTypeGlyphContour(segments);
        }
    }
}
