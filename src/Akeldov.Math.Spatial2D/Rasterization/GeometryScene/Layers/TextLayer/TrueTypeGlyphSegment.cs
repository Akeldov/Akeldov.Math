using System.Runtime.InteropServices;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct TrueTypeGlyphSegment
    {
        private TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind kind, PointXY startPoint, PointXY controlPoint, PointXY endPoint)
        {
            Kind = kind;
            StartPoint = startPoint;
            ControlPoint = controlPoint;
            EndPoint = endPoint;
        }

        public TrueTypeGlyphSegmentKind Kind { get; }

        public PointXY StartPoint { get; }

        public PointXY ControlPoint { get; }

        public PointXY EndPoint { get; }

        public static TrueTypeGlyphSegment Line(PointXY startPoint, PointXY endPoint) =>
            new TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind.Line, startPoint, startPoint, endPoint);

        public static TrueTypeGlyphSegment Quadratic(PointXY startPoint, PointXY controlPoint, PointXY endPoint) =>
            new TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind.Quadratic, startPoint, controlPoint, endPoint);

        public TrueTypeGlyphSegment Transform(float a, float b, float c, float d, float dx, float dy)
        {
            return new TrueTypeGlyphSegment(
                Kind,
                Transform(StartPoint, a, b, c, d, dx, dy),
                Transform(ControlPoint, a, b, c, d, dx, dy),
                Transform(EndPoint, a, b, c, d, dx, dy));
        }

        private static PointXY Transform(PointXY point, float a, float b, float c, float d, float dx, float dy)
        {
            return new PointXY(
                point.X * a + point.Y * b + dx,
                point.X * c + point.Y * d + dy);
        }
    }
}
