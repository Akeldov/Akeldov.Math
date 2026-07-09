using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class TrueTypeTextLayout
    {
        public static TextSignedDistanceProvider CreateText(
            TrueTypeFont font,
            string text,
            PointXY origin,
            float fontSize,
            TextLayoutOptions options)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            PointXYValidation.ThrowIfNotFinite(
                origin,
                nameof(origin),
                "Text origin coordinates must be finite.");

            if (fontSize <= 0f || float.IsNaN(fontSize) || float.IsInfinity(fontSize))
                throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be finite and positive.");

            if (float.IsNaN(options.LetterSpacing) || float.IsInfinity(options.LetterSpacing))
                throw new ArgumentOutOfRangeException(nameof(options), "Text layout letter spacing must be finite.");

            if (float.IsNaN(options.LineSpacing) || float.IsInfinity(options.LineSpacing))
                throw new ArgumentOutOfRangeException(nameof(options), "Text layout line spacing must be finite.");

            ValidateAnchor(options.Anchor);

            float scale = fontSize / font.UnitsPerEm;
            float lineAdvance = (font.Ascender - font.Descender + font.LineGap) * scale + options.LineSpacing;
            if (lineAdvance <= 0f || float.IsNaN(lineAdvance) || float.IsInfinity(lineAdvance))
                throw new ArgumentOutOfRangeException(nameof(options), "Text layout line advance must be finite and positive.");

            var contours = new List<TextContour>();
            var bounds = new TextBounds();
            float maxLineAdvance = 0f;
            string[] lines = SplitLines(text);

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                float baselineY = -lineIndex * lineAdvance;
                float lineAdvanceWidth = AddLine(
                    font,
                    lines[lineIndex],
                    baselineY,
                    scale,
                    options,
                    contours,
                    bounds);

                if (lineAdvanceWidth > maxLineAdvance)
                    maxLineAdvance = lineAdvanceWidth;
            }

            float fallbackTop = font.Ascender * scale;
            float fallbackBottom = font.Descender * scale - MathF.Max(0, lines.Length - 1) * lineAdvance;
            float left = bounds.HasValue ? bounds.MinX : 0f;
            float right = bounds.HasValue ? bounds.MaxX : maxLineAdvance;
            float top = bounds.HasValue ? bounds.MaxY : fallbackTop;
            float bottom = bounds.HasValue ? bounds.MinY : fallbackBottom;

            VectorXY offset = GetAnchorOffset(origin, options.Anchor, maxLineAdvance, left, right, top, bottom);
            var compositeContours = new List<IContour>();
            for (int i = 0; i < contours.Count; i++)
            {
                CompositeContour? contour = contours[i].ToCompositeContour(offset);
                if (contour != null)
                    compositeContours.Add(contour);
            }

            return new TextSignedDistanceProvider(compositeContours);
        }

        private static float AddLine(
            TrueTypeFont font,
            string line,
            float baselineY,
            float scale,
            TextLayoutOptions options,
            List<TextContour> contours,
            TextBounds bounds)
        {
            List<int> codePoints = GetCodePoints(line);
            float penX = 0f;
            int previousGlyphIndex = -1;

            for (int i = 0; i < codePoints.Count; i++)
            {
                int glyphIndex = font.GetGlyphIndex(codePoints[i]);
                if (previousGlyphIndex >= 0 && options.UseKerning)
                    penX += font.GetKerning(previousGlyphIndex, glyphIndex) * scale;

                AddGlyph(font.GetGlyphOutline(glyphIndex), penX, baselineY, scale, contours, bounds);
                penX += font.GetAdvanceWidth(glyphIndex) * scale;
                if (i < codePoints.Count - 1)
                    penX += options.LetterSpacing;

                previousGlyphIndex = glyphIndex;
            }

            return penX;
        }

        private static void AddGlyph(
            TrueTypeGlyphOutline glyph,
            float penX,
            float baselineY,
            float scale,
            List<TextContour> contours,
            TextBounds bounds)
        {
            for (int contourIndex = 0; contourIndex < glyph.Contours.Count; contourIndex++)
            {
                TrueTypeGlyphContour sourceContour = glyph.Contours[contourIndex];
                var targetContour = new TextContour();
                for (int segmentIndex = 0; segmentIndex < sourceContour.Segments.Count; segmentIndex++)
                {
                    TrueTypeGlyphSegment segment = sourceContour.Segments[segmentIndex];
                    TextSegment textSegment = segment.Kind == TrueTypeGlyphSegmentKind.Line
                        ? TextSegment.Line(
                            ToWorld(segment.StartPoint, penX, baselineY, scale),
                            ToWorld(segment.EndPoint, penX, baselineY, scale))
                        : TextSegment.Quadratic(
                            ToWorld(segment.StartPoint, penX, baselineY, scale),
                            ToWorld(segment.ControlPoint, penX, baselineY, scale),
                            ToWorld(segment.EndPoint, penX, baselineY, scale));

                    targetContour.Add(textSegment);
                    bounds.Include(textSegment.StartPoint);
                    bounds.Include(textSegment.ControlPoint);
                    bounds.Include(textSegment.EndPoint);
                }

                if (targetContour.Count > 0)
                    contours.Add(targetContour);
            }
        }

        private static PointXY ToWorld(PointXY fontPoint, float penX, float baselineY, float scale)
        {
            return new PointXY(
                penX + fontPoint.X * scale,
                baselineY + fontPoint.Y * scale);
        }

        private static VectorXY GetAnchorOffset(
            PointXY origin,
            TextAnchor anchor,
            float maxLineAdvance,
            float left,
            float right,
            float top,
            float bottom)
        {
            switch (anchor)
            {
                case TextAnchor.BaselineLeft:
                    return new VectorXY(origin.X, origin.Y);
                case TextAnchor.BaselineCenter:
                    return new VectorXY(origin.X - maxLineAdvance * 0.5f, origin.Y);
                case TextAnchor.BaselineRight:
                    return new VectorXY(origin.X - maxLineAdvance, origin.Y);
                case TextAnchor.TopLeft:
                    return new VectorXY(origin.X - left, origin.Y - top);
                case TextAnchor.TopCenter:
                    return new VectorXY(origin.X - (left + right) * 0.5f, origin.Y - top);
                case TextAnchor.TopRight:
                    return new VectorXY(origin.X - right, origin.Y - top);
                case TextAnchor.CenterLeft:
                    return new VectorXY(origin.X - left, origin.Y - (top + bottom) * 0.5f);
                case TextAnchor.Center:
                    return new VectorXY(origin.X - (left + right) * 0.5f, origin.Y - (top + bottom) * 0.5f);
                case TextAnchor.CenterRight:
                    return new VectorXY(origin.X - right, origin.Y - (top + bottom) * 0.5f);
                case TextAnchor.BottomLeft:
                    return new VectorXY(origin.X - left, origin.Y - bottom);
                case TextAnchor.BottomCenter:
                    return new VectorXY(origin.X - (left + right) * 0.5f, origin.Y - bottom);
                case TextAnchor.BottomRight:
                    return new VectorXY(origin.X - right, origin.Y - bottom);
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), "Text anchor is not supported.");
            }
        }

        private static void ValidateAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.BaselineLeft:
                case TextAnchor.BaselineCenter:
                case TextAnchor.BaselineRight:
                case TextAnchor.TopLeft:
                case TextAnchor.TopCenter:
                case TextAnchor.TopRight:
                case TextAnchor.CenterLeft:
                case TextAnchor.Center:
                case TextAnchor.CenterRight:
                case TextAnchor.BottomLeft:
                case TextAnchor.BottomCenter:
                case TextAnchor.BottomRight:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), "Text anchor is not supported.");
            }
        }

        private static string[] SplitLines(string text)
        {
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static List<int> GetCodePoints(string text)
        {
            var codePoints = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoints.Add(char.ConvertToUtf32(c, text[++i]));
                    continue;
                }

                codePoints.Add(c);
            }

            return codePoints;
        }

        private sealed class TextBounds
        {
            public bool HasValue { get; private set; }

            public float MinX { get; private set; }

            public float MinY { get; private set; }

            public float MaxX { get; private set; }

            public float MaxY { get; private set; }

            public void Include(PointXY point)
            {
                if (!HasValue)
                {
                    MinX = point.X;
                    MaxX = point.X;
                    MinY = point.Y;
                    MaxY = point.Y;
                    HasValue = true;
                    return;
                }

                if (point.X < MinX)
                    MinX = point.X;
                if (point.X > MaxX)
                    MaxX = point.X;
                if (point.Y < MinY)
                    MinY = point.Y;
                if (point.Y > MaxY)
                    MaxY = point.Y;
            }
        }

        private sealed class TextContour
        {
            private readonly List<TextSegment> _segments = new List<TextSegment>();

            public int Count => _segments.Count;

            public void Add(TextSegment segment)
            {
                _segments.Add(segment);
            }

            public CompositeContour? ToCompositeContour(VectorXY offset)
            {
                var paths = new List<IFinitePath>(_segments.Count);
                for (int i = 0; i < _segments.Count; i++)
                {
                    TextSegment segment = _segments[i].Translate(offset);
                    if (segment.Kind == TextSegmentKind.Line)
                    {
                        paths.Add(new ParameterizedSegment(segment.StartPoint, segment.EndPoint));
                    }
                    else
                    {
                        paths.Add(new QuadraticBezier(segment.StartPoint, segment.ControlPoint, segment.EndPoint));
                    }
                }

                return paths.Count == 0 ? null : new CompositeContour(paths);
            }
        }

        private readonly struct TextSegment
        {
            private TextSegment(TextSegmentKind kind, PointXY startPoint, PointXY controlPoint, PointXY endPoint)
            {
                Kind = kind;
                StartPoint = startPoint;
                ControlPoint = controlPoint;
                EndPoint = endPoint;
            }

            public TextSegmentKind Kind { get; }

            public PointXY StartPoint { get; }

            public PointXY ControlPoint { get; }

            public PointXY EndPoint { get; }

            public static TextSegment Line(PointXY startPoint, PointXY endPoint)
            {
                return new TextSegment(TextSegmentKind.Line, startPoint, startPoint, endPoint);
            }

            public static TextSegment Quadratic(PointXY startPoint, PointXY controlPoint, PointXY endPoint)
            {
                return new TextSegment(TextSegmentKind.Quadratic, startPoint, controlPoint, endPoint);
            }

            public TextSegment Translate(VectorXY offset)
            {
                return new TextSegment(
                    Kind,
                    StartPoint + offset,
                    ControlPoint + offset,
                    EndPoint + offset);
            }
        }

        private enum TextSegmentKind
        {
            Line,
            Quadratic
        }
    }
}
