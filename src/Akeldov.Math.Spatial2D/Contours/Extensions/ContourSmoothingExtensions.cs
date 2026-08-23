using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Provides contour smoothing helpers.
    /// </summary>
    public static class ContourSmoothingExtensions
    {
        /// <summary>
        /// Returns a contour with fillet arcs inserted at corners where two adjacent parameterized segments meet.
        /// </summary>
        /// <param name="contour">The contour to fillet.</param>
        /// <param name="radius">The radius of inserted fillet arcs.</param>
        /// <returns>A new contour with segment-segment corners filleted by parameterized arcs.</returns>
        public static CompositeContour FilletCorners(this ICompositeContour contour, float radius)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));

            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");

            IReadOnlyList<IContourPath> curves = contour.Curves;
            if (curves == null || curves.Count == 0)
                throw new InvalidOperationException("CompositeContour must expose at least one contour path.");

            ParameterizedArc?[] cornerArcs = CreateCornerArcs(curves, radius);
            var smoothedCurves = new List<IContourPath>(curves.Count + cornerArcs.Length);

            for (int i = 0; i < curves.Count; i++)
            {
                IContourPath curve = curves[i];
                ParameterizedArc? startArc = cornerArcs[GetPreviousIndex(i, curves.Count)];
                ParameterizedArc? endArc = cornerArcs[i];

                if (curve is ParameterizedSegment segment)
                {
                    smoothedCurves.Add(CreateTrimmedSegment(segment, startArc, endArc));
                }
                else
                {
                    smoothedCurves.Add(curve);
                }

                if (endArc.HasValue)
                    smoothedCurves.Add(endArc.Value);
            }

            return new CompositeContour(smoothedCurves);
        }

        private static ParameterizedArc?[] CreateCornerArcs(IReadOnlyList<IContourPath> curves, float radius)
        {
            var cornerArcs = new ParameterizedArc?[curves.Count];

            for (int i = 0; i < curves.Count; i++)
            {
                int nextIndex = (i + 1) % curves.Count;

                if (curves[i] is ParameterizedSegment previousSegment &&
                    curves[nextIndex] is ParameterizedSegment nextSegment &&
                    previousSegment.EndPoint.AlmostEquals(nextSegment.StartPoint) &&
                    TryCreateFilletArc(previousSegment, nextSegment, radius, out ParameterizedArc arc))
                {
                    cornerArcs[i] = arc;
                }
            }

            return cornerArcs;
        }

        private static bool TryCreateFilletArc(
            ParameterizedSegment previousSegment,
            ParameterizedSegment nextSegment,
            float radius,
            out ParameterizedArc arc)
        {
            try
            {
                Arc tangentArc = CornerExtensions.CreateFilletArc(
                    previousSegment.StartPoint,
                    previousSegment.EndPoint,
                    nextSegment.EndPoint,
                    radius);

                PointXY startPoint = GetCircleTangentPointOnSegmentLine(
                    tangentArc.Center,
                    previousSegment,
                    previousSegment.EndPoint);

                PointXY endPoint = GetCircleTangentPointOnSegmentLine(
                    tangentArc.Center,
                    nextSegment,
                    nextSegment.StartPoint);

                float startAngle = GetAngle(tangentArc.Center, startPoint);
                float endAngle = GetAngle(tangentArc.Center, endPoint);
                AngularDirection direction = GetShortestAngularDirection(startAngle, endAngle);

                arc = new ParameterizedArc(
                    tangentArc.Center,
                    tangentArc.Radius,
                    startAngle,
                    endAngle,
                    direction);

                return true;
            }
            catch (ArgumentException)
            {
                arc = default;
                return false;
            }
        }

        private static ParameterizedSegment CreateTrimmedSegment(
            ParameterizedSegment segment,
            ParameterizedArc? startArc,
            ParameterizedArc? endArc)
        {
            PointXY startPoint = segment.StartPoint;
            PointXY endPoint = segment.EndPoint;
            bool includesStartPoint = segment.IncludesStartPoint;
            bool includesEndPoint = segment.IncludesEndPoint;

            if (startArc.HasValue)
            {
                startPoint = startArc.Value.EndPoint;
                includesStartPoint = false;
            }

            if (endArc.HasValue)
            {
                endPoint = endArc.Value.StartPoint;
                includesEndPoint = false;
            }

            return new ParameterizedSegment(startPoint, endPoint, includesStartPoint, includesEndPoint);
        }

        private static PointXY GetCircleTangentPointOnSegmentLine(
            PointXY circleCenter,
            ParameterizedSegment segment,
            PointXY fallbackPoint)
        {
            if (segment.Length <= GeometryConstants.GeometryEpsilon)
                return fallbackPoint;

            var line = new Line(segment.StartPoint, segment.EndPoint);
            return line.Project(circleCenter).ProjectedPoint;
        }

        private static float GetAngle(PointXY center, PointXY point)
        {
            return MathF.Atan2((point - center).Y, (point - center).X).NormalizeAngleRad();
        }

        private static AngularDirection GetShortestAngularDirection(float startAngle, float endAngle)
        {
            return PositiveAngleDelta(startAngle, endAngle) <= MathF.PI
                ? AngularDirection.Counterclockwise
                : AngularDirection.Clockwise;
        }

        private static float PositiveAngleDelta(float from, float to)
        {
            float delta = to - from;
            if (delta < 0f)
                delta += 2f * MathF.PI;

            return delta;
        }

        private static int GetPreviousIndex(int index, int count)
        {
            return index == 0 ? count - 1 : index - 1;
        }
    }
}
