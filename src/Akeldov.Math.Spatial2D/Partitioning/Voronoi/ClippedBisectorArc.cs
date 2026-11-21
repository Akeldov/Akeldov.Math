using System;
using Akeldov.Math.Spatial2D.Curves;
using static System.MathF;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    // A clipped circular arc can have a radius millions of times its chord length for adjacent
    // float weights. Store its endpoints and sweep so every contour joins at shared float vertices.
    // The arrangement splits at the four cardinal directions, so each arc is monotone in x and y.
    internal sealed class ClippedBisectorArc : IContourPath
    {
        private readonly float _dx, _dy;
        private readonly float _chord, _halfSweep, _sinHalf, _cosHalf;

        /// <param name="start">First endpoint on the clipped boundary.</param>
        /// <param name="end">Last endpoint on the clipped boundary.</param>
        /// <param name="signedSweep">Signed arc sweep in radians.</param>
        public ClippedBisectorArc(PointXY start, PointXY end, float signedSweep)
        {
            StartPoint = start;
            EndPoint = end;
            _dx = end.X - start.X;
            _dy = end.Y - start.Y;
            _chord = start.Distance(end);
            _halfSweep = signedSweep * 0.5f;
            _sinHalf = Sin(_halfSweep);
            _cosHalf = Cos(_halfSweep);
            Length = _sinHalf == 0 ? _chord : _chord * (_halfSweep / _sinHalf);
        }

        public PointXY StartPoint { get; }
        public PointXY EndPoint { get; }
        public PointXY EndpointA => StartPoint;
        public PointXY EndpointB => EndPoint;
        public float Length { get; }

        public PointXY GetPoint(float curveCoordinate)
        {
            if (!(curveCoordinate >= 0 && curveCoordinate <= Length))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate));
            return AtFraction(curveCoordinate / Length);
        }

        private PointXY AtFraction(float f)
        {
            if (f <= 0)
                return StartPoint;
            if (f >= 1)
                return EndPoint;
            bool fromStart = f <= 0.5f;
            float fraction = fromStart ? f : 1 - f;
            // At small angles the sine ratio equals the arc fraction to float precision.
            float scale = Abs(_halfSweep) < 0.0001f ? fraction : Sin(fraction * _halfSweep) / _sinHalf;
            float remaining = (1 - fraction) * _halfSweep;
            float along = scale * Cos(remaining);
            float across = scale * Sin(remaining);
            var endpoint = fromStart ? StartPoint : EndPoint;
            if (!fromStart)
                along = -along;
            return new PointXY(endpoint.X + along * _dx + across * _dy,
                endpoint.Y + along * _dy - across * _dx);
        }

        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");
            if (point == StartPoint)
                return new ParameterizedCurveProjection(StartPoint, 0, 0);
            if (point == EndPoint)
                return new ParameterizedCurveProjection(EndPoint, Length, 0);
            double x = (double)point.X - StartPoint.X - 0.5 * _dx;
            double y = (double)point.Y - StartPoint.Y - 0.5 * _dy;
            double along = (x * _dx + y * _dy) / _chord;
            double normal = (x * _dy - y * _dx) / _chord;
            double offset = _halfSweep == 0 ? along / _chord
                : System.Math.Atan2(along * _sinHalf, 0.5 * _chord * _cosHalf + normal * _sinHalf) / (2 * _halfSweep);
            float fraction = (float)System.Math.Max(0, System.Math.Min(1, 0.5 + offset));
            var projected = AtFraction(fraction);
            return new ParameterizedCurveProjection(projected, fraction * Length, point.Distance(projected));
        }

        public CurveProjection Project(PointXY point)
        {
            var projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        public float Distance(PointXY point) => ProjectWithParameter(point).Distance;

        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");
            if ((origin.Y < StartPoint.Y) == (origin.Y < EndPoint.Y))
                return 0;
            if (origin.Y == StartPoint.Y)
                return StartPoint.X > origin.X ? 1 : 0;
            if (origin.Y == EndPoint.Y)
                return EndPoint.X > origin.X ? 1 : 0;

            if (origin.X < Min(StartPoint.X, EndPoint.X))
                return 1;
            if (origin.X >= Max(StartPoint.X, EndPoint.X))
                return 0;

            // Each arc lies in one quadrant. The sign of the circle equation at the
            // ray origin therefore identifies its side without solving for the intersection.
            double ax = (double)origin.X - StartPoint.X, ay = (double)origin.Y - StartPoint.Y;
            double bx = (double)origin.X - EndPoint.X, by = (double)origin.Y - EndPoint.Y;
            double cross = ax * by - ay * bx;
            double side = _sinHalf * (ax * bx + ay * by) - _cosHalf * cross;
            bool crosses = EndPoint.Y > StartPoint.Y ? side < 0 : side > 0;
            return crosses ? 1 : 0;
        }
    }
}
