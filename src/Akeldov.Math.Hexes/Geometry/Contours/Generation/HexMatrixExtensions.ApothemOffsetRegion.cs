using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Geometry.Contours
{
    /// <summary>
    /// Builds world-space regions offset from polyhex boundaries.
    /// </summary>
    public static partial class HexMatrixExtensions
    {
        /// <summary>
        /// Creates a filled region offset outward from the polyhex boundary by one hex apothem using the default <see cref="Layout.OddR"/> layout.
        /// </summary>
        /// <typeparam name="TPolyhexGeometry">The polyhex geometry type.</typeparam>
        /// <param name="polyhexGeometry">The polyhex geometry whose occupied cells define the source region.</param>
        /// <returns>
        /// A contour-based region whose contours are offset from the source region boundary by
        /// <see cref="IPolyhexGeometry.HexApothem"/>. Source regions with holes or multiple disconnected boundary
        /// chains are represented by multiple offset contours and the even-odd fill rule.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polyhexGeometry"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the polyhex contains no occupied cells or offset boundary sections cannot be formed.</exception>
        public static ContourBasedRegion ToApothemOffsetRegion<TPolyhexGeometry>(this TPolyhexGeometry polyhexGeometry)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            return polyhexGeometry.ToApothemOffsetRegion(Layout.OddR);
        }

        /// <summary>
        /// Creates a filled region offset outward from the polyhex boundary by one hex apothem using the specified hex layout.
        /// </summary>
        /// <typeparam name="TPolyhexGeometry">The polyhex geometry type.</typeparam>
        /// <param name="polyhexGeometry">The polyhex geometry whose occupied cells define the source region.</param>
        /// <param name="layout">The layout used to map hex indices to world-space boundary segments.</param>
        /// <returns>
        /// A contour-based region whose contours are offset from the source region boundary by
        /// <see cref="IPolyhexGeometry.HexApothem"/>. Source regions with holes or multiple disconnected boundary
        /// chains are represented by multiple offset contours and the even-odd fill rule.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polyhexGeometry"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="layout"/> is not a defined layout.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the polyhex contains no occupied cells or offset boundary sections cannot be formed.</exception>
        public static ContourBasedRegion ToApothemOffsetRegion<TPolyhexGeometry>(
            this TPolyhexGeometry polyhexGeometry,
            Layout layout)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            if (polyhexGeometry is null)
                throw new ArgumentNullException(nameof(polyhexGeometry));

            ContourBasedRegion sourceRegion = polyhexGeometry.ToRegion(layout);
            var offsetCurves = new List<IFinitePath>();

            for (int i = 0; i < sourceRegion.Contours.Count; i++)
            {
                AddOffsetCandidateCurves(
                    sourceRegion.Contours[i],
                    polyhexGeometry.HexApothem,
                    offsetCurves);
            }

            return CreateOuterOffsetRegion(sourceRegion, offsetCurves, polyhexGeometry.HexApothem);
        }

        private static void AddOffsetCandidateCurves(
            IContour sourceContour,
            float offsetDistance,
            List<IFinitePath> offsetCurves)
        {
            ParameterizedSegment[] sourceSegments = GetSourceSegments(sourceContour);
            ParameterizedSegment[] offsetSegments = OffsetOutward(sourceSegments, offsetDistance);
            OffsetJoin[] joins = CreateOffsetJoins(sourceSegments, offsetSegments, offsetDistance);

            for (int i = 0; i < offsetSegments.Length; i++)
            {
                int previousJoinIndex = GetPreviousIndex(i, offsetSegments.Length);

                offsetCurves.Add(new ParameterizedSegment(
                    joins[previousJoinIndex].NextStartPoint,
                    joins[i].PreviousEndPoint,
                    offsetSegments[i].IncludesStartPoint,
                    offsetSegments[i].IncludesEndPoint));

                if (joins[i].Arc is ParameterizedArc arc)
                    offsetCurves.Add(arc);
            }
        }

        private static ParameterizedSegment[] GetSourceSegments(IContour contour)
        {
            ICompositeContour? compositeContour = contour as ICompositeContour;
            if (compositeContour == null)
            {
                throw new InvalidOperationException(
                    "Polyhex source contour must be a composite contour.");
            }

            var segments = new ParameterizedSegment[compositeContour.Curves.Count];

            for (int i = 0; i < compositeContour.Curves.Count; i++)
            {
                if (!(compositeContour.Curves[i] is ParameterizedSegment segment))
                {
                    throw new InvalidOperationException(
                        "Polyhex source contour must consist only of parameterized segments.");
                }

                segments[i] = segment;
            }

            return segments;
        }

        private static ParameterizedSegment[] OffsetOutward(
            ParameterizedSegment[] sourceSegments,
            float offsetDistance)
        {
            var offsetSegments = new ParameterizedSegment[sourceSegments.Length];

            for (int i = 0; i < sourceSegments.Length; i++)
                offsetSegments[i] = OffsetOutward(sourceSegments[i], offsetDistance);

            return offsetSegments;
        }

        private static ParameterizedSegment OffsetOutward(ParameterizedSegment segment, float offsetDistance)
        {
            VectorXY direction = (segment.EndPoint - segment.StartPoint).Normalize();
            var outwardNormal = new VectorXY(direction.Y, -direction.X);

            return segment + outwardNormal * offsetDistance;
        }

        private static OffsetJoin[] CreateOffsetJoins(
            ParameterizedSegment[] sourceSegments,
            ParameterizedSegment[] offsetSegments,
            float radius)
        {
            var joins = new OffsetJoin[offsetSegments.Length];

            for (int i = 0; i < offsetSegments.Length; i++)
            {
                int nextIndex = (i + 1) % offsetSegments.Length;
                joins[i] = CreateOffsetJoin(
                    sourceSegments[i],
                    sourceSegments[nextIndex],
                    offsetSegments[i],
                    offsetSegments[nextIndex],
                    radius);
            }

            return joins;
        }

        private static OffsetJoin CreateOffsetJoin(
            ParameterizedSegment sourceSegment,
            ParameterizedSegment nextSourceSegment,
            ParameterizedSegment offsetSegment,
            ParameterizedSegment nextOffsetSegment,
            float radius)
        {
            if (offsetSegment.EndPoint.AlmostEquals(nextOffsetSegment.StartPoint))
                return new OffsetJoin(offsetSegment.EndPoint, nextOffsetSegment.StartPoint, null);

            VectorXY sourceDirection = (sourceSegment.EndPoint - sourceSegment.StartPoint).Normalize();
            VectorXY nextSourceDirection = (nextSourceSegment.EndPoint - nextSourceSegment.StartPoint).Normalize();
            float turn = VectorXY.Cross(sourceDirection, nextSourceDirection);

            if (turn.IsAlmostZero())
                return new OffsetJoin(offsetSegment.EndPoint, nextOffsetSegment.StartPoint, null);

            if (turn < 0f && TryGetLineIntersection(offsetSegment, nextOffsetSegment, out PointXY intersection))
                return new OffsetJoin(intersection, intersection, null);

            PointXY center = sourceSegment.EndPoint;
            float startAngle = GetAngle(center, offsetSegment.EndPoint);
            float endAngle = GetAngle(center, nextOffsetSegment.StartPoint);
            const AngularDirection direction = AngularDirection.Counterclockwise;

            return new OffsetJoin(
                offsetSegment.EndPoint,
                nextOffsetSegment.StartPoint,
                new ParameterizedArc(center, radius, startAngle, endAngle, direction));
        }

        private static bool TryGetLineIntersection(
            ParameterizedSegment first,
            ParameterizedSegment second,
            out PointXY intersection)
        {
            VectorXY firstDirection = first.EndPoint - first.StartPoint;
            VectorXY secondDirection = second.EndPoint - second.StartPoint;
            float cross = VectorXY.Cross(firstDirection, secondDirection);

            if (cross.IsAlmostZero())
            {
                intersection = default;
                return false;
            }

            VectorXY originDelta = second.StartPoint - first.StartPoint;
            float coordinate = VectorXY.Cross(originDelta, secondDirection) / cross;
            intersection = first.StartPoint + firstDirection * coordinate;
            return true;
        }

        private static float GetAngle(PointXY center, PointXY point)
        {
            return MathF.Atan2(point.Y - center.Y, point.X - center.X);
        }

        private static ContourBasedRegion CreateOuterOffsetRegion(
            ContourBasedRegion sourceRegion,
            IReadOnlyList<IFinitePath> candidateCurves,
            float offsetDistance)
        {
            List<IFinitePath> sections = SplitCurvesAtIntersections(candidateCurves);
            var boundarySections = new List<IFinitePath>(sections.Count);
            float offsetEpsilon = GetOffsetEpsilon(offsetDistance);

            for (int i = 0; i < sections.Count; i++)
            {
                IFinitePath section = sections[i];
                if (section.Length <= offsetEpsilon)
                    continue;

                PointXY midpoint = section.GetPoint(section.Length * 0.5f);
                if (sourceRegion.Distance(midpoint) >= offsetDistance - offsetEpsilon)
                    boundarySections.Add(section);
            }

            if (boundarySections.Count == 0)
                throw new InvalidOperationException("Polyhex offset contour does not contain outer boundary sections.");

            return new ContourBasedRegion(OrderClosedContours(boundarySections));
        }

        private static List<IFinitePath> SplitCurvesAtIntersections(IReadOnlyList<IFinitePath> curves)
        {
            var splitCoordinates = new List<float>[curves.Count];

            for (int i = 0; i < curves.Count; i++)
            {
                splitCoordinates[i] = new List<float> { 0f, curves[i].Length };
            }

            for (int i = 0; i < curves.Count; i++)
            {
                for (int j = i + 1; j < curves.Count; j++)
                {
                    foreach (PointXY intersection in GetIntersections(curves[i], curves[j]))
                    {
                        AddSplitCoordinate(splitCoordinates[i], curves[i], intersection);
                        AddSplitCoordinate(splitCoordinates[j], curves[j], intersection);
                    }
                }
            }

            var sections = new List<IFinitePath>();

            for (int i = 0; i < curves.Count; i++)
            {
                splitCoordinates[i].Sort();

                for (int j = 0; j + 1 < splitCoordinates[i].Count; j++)
                {
                    float startCoordinate = splitCoordinates[i][j];
                    float endCoordinate = splitCoordinates[i][j + 1];

                    if (endCoordinate - startCoordinate <= GeometryConstants.GeometryEpsilon)
                        continue;

                    sections.Add(CreateCurveSection(curves[i], startCoordinate, endCoordinate));
                }
            }

            return sections;
        }

        private static void AddSplitCoordinate(
            List<float> splitCoordinates,
            IFinitePath curve,
            PointXY intersection)
        {
            ParameterizedCurveProjection projection = curve.ProjectWithParameter(intersection);
            float coordinate = MathF.Max(0f, MathF.Min(curve.Length, projection.CurveCoordinate));

            for (int i = 0; i < splitCoordinates.Count; i++)
            {
                if (splitCoordinates[i].AlmostEquals(coordinate))
                    return;
            }

            splitCoordinates.Add(coordinate);
        }

        private static IFinitePath CreateCurveSection(
            IFinitePath curve,
            float startCoordinate,
            float endCoordinate)
        {
            PointXY startPoint = curve.GetPoint(startCoordinate);
            PointXY endPoint = curve.GetPoint(endCoordinate);

            if (curve is ParameterizedArc arc)
            {
                return new ParameterizedArc(
                    arc.Center,
                    arc.Radius,
                    GetAngle(arc.Center, startPoint),
                    GetAngle(arc.Center, endPoint),
                    arc.AngularDirection);
            }

            return new ParameterizedSegment(startPoint, endPoint, true, false);
        }

        private static List<PointXY> GetIntersections(IFinitePath first, IFinitePath second)
        {
            var intersections = new List<PointXY>();

            if (first is ParameterizedSegment firstSegment &&
                second is ParameterizedSegment secondSegment)
            {
                AddSegmentSegmentIntersections(intersections, firstSegment, secondSegment);
            }
            else if (first is ParameterizedSegment segment &&
                second is ParameterizedArc arc)
            {
                AddSegmentArcIntersections(intersections, segment, arc);
            }
            else if (first is ParameterizedArc firstArc &&
                second is ParameterizedSegment segmentB)
            {
                AddSegmentArcIntersections(intersections, segmentB, firstArc);
            }
            else if (first is ParameterizedArc arcA &&
                second is ParameterizedArc arcB)
            {
                AddArcArcIntersections(intersections, arcA, arcB);
            }

            return intersections;
        }

        private static void AddSegmentSegmentIntersections(
            List<PointXY> intersections,
            ParameterizedSegment first,
            ParameterizedSegment second)
        {
            VectorXY firstDirection = first.EndPoint - first.StartPoint;
            VectorXY secondDirection = second.EndPoint - second.StartPoint;
            float cross = VectorXY.Cross(firstDirection, secondDirection);

            if (cross.IsAlmostZero())
                return;

            VectorXY originDelta = second.StartPoint - first.StartPoint;
            float firstCoordinate = VectorXY.Cross(originDelta, secondDirection) / cross;
            float secondCoordinate = VectorXY.Cross(originDelta, firstDirection) / cross;

            if (IsNormalizedCoordinateInside(firstCoordinate) &&
                IsNormalizedCoordinateInside(secondCoordinate))
            {
                AddDistinct(intersections, first.StartPoint + firstDirection * firstCoordinate);
            }
        }

        private static void AddSegmentArcIntersections(
            List<PointXY> intersections,
            ParameterizedSegment segment,
            ParameterizedArc arc)
        {
            VectorXY direction = segment.EndPoint - segment.StartPoint;
            VectorXY startToCenter = segment.StartPoint - arc.Center;

            float a = VectorXY.Dot(direction, direction);
            if (a <= GeometryConstants.GeometryEpsilonSquared)
                return;

            float b = 2f * VectorXY.Dot(startToCenter, direction);
            float c = startToCenter.SquaredLength - arc.Radius * arc.Radius;
            float discriminant = b * b - 4f * a * c;

            if (discriminant < -GeometryConstants.GeometryEpsilon)
                return;

            if (discriminant < 0f)
                discriminant = 0f;

            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            AddSegmentArcIntersection(intersections, segment, arc, (-b - sqrtDiscriminant) / (2f * a));
            AddSegmentArcIntersection(intersections, segment, arc, (-b + sqrtDiscriminant) / (2f * a));
        }

        private static void AddSegmentArcIntersection(
            List<PointXY> intersections,
            ParameterizedSegment segment,
            ParameterizedArc arc,
            float normalizedCoordinate)
        {
            if (!IsNormalizedCoordinateInside(normalizedCoordinate))
                return;

            PointXY point = segment.StartPoint + (segment.EndPoint - segment.StartPoint) * normalizedCoordinate;
            if (arc.IsWithinAngularRegion(point))
                AddDistinct(intersections, point);
        }

        private static void AddArcArcIntersections(
            List<PointXY> intersections,
            ParameterizedArc first,
            ParameterizedArc second)
        {
            VectorXY centerDelta = second.Center - first.Center;
            float centerDistance = centerDelta.Length;

            if (centerDistance <= GeometryConstants.GeometryEpsilon)
                return;

            if (centerDistance > first.Radius + second.Radius + GeometryConstants.GeometryEpsilon)
                return;

            if (centerDistance < MathF.Abs(first.Radius - second.Radius) - GeometryConstants.GeometryEpsilon)
                return;

            float a = (first.Radius * first.Radius -
                second.Radius * second.Radius +
                centerDistance * centerDistance) / (2f * centerDistance);
            float hSquared = first.Radius * first.Radius - a * a;

            if (hSquared < -GeometryConstants.GeometryEpsilon)
                return;

            if (hSquared < 0f)
                hSquared = 0f;

            VectorXY direction = centerDelta / centerDistance;
            PointXY basePoint = first.Center + direction * a;
            VectorXY perpendicular = new VectorXY(-direction.Y, direction.X) * MathF.Sqrt(hSquared);

            AddArcArcIntersection(intersections, first, second, basePoint + perpendicular);
            AddArcArcIntersection(intersections, first, second, basePoint - perpendicular);
        }

        private static void AddArcArcIntersection(
            List<PointXY> intersections,
            ParameterizedArc first,
            ParameterizedArc second,
            PointXY point)
        {
            if (first.IsWithinAngularRegion(point) && second.IsWithinAngularRegion(point))
                AddDistinct(intersections, point);
        }

        private static void AddDistinct(List<PointXY> points, PointXY point)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].AlmostEquals(point))
                    return;
            }

            points.Add(point);
        }

        private static bool IsNormalizedCoordinateInside(float coordinate)
        {
            return coordinate >= -GeometryConstants.GeometryEpsilon &&
                coordinate <= 1f + GeometryConstants.GeometryEpsilon;
        }

        private static List<IContour> OrderClosedContours(IReadOnlyList<IFinitePath> curves)
        {
            var used = new bool[curves.Count];
            var contours = new List<IContour>();

            for (int i = 0; i < curves.Count; i++)
            {
                if (used[i])
                    continue;

                List<IFinitePath> chain = BuildClosedChain(curves, used, i);
                contours.Add(new CompositeContour(chain));
            }

            if (contours.Count == 0)
                throw new InvalidOperationException("Polyhex offset contour must contain closed chains.");

            return contours;
        }

        private static List<IFinitePath> BuildClosedChain(
            IReadOnlyList<IFinitePath> curves,
            bool[] used,
            int startIndex)
        {
            var chain = new List<IFinitePath>();

            used[startIndex] = true;
            chain.Add(curves[startIndex]);

            PointXY startPoint = curves[startIndex].StartPoint;
            PointXY currentPoint = curves[startIndex].EndPoint;

            while (!currentPoint.AlmostEquals(startPoint))
            {
                int nextIndex = FindNextCurve(curves, used, currentPoint, out bool reverse);
                if (nextIndex < 0)
                    throw new InvalidOperationException("Polyhex offset contour sections must form closed chains.");

                used[nextIndex] = true;

                IFinitePath nextCurve = reverse ? ReverseCurve(curves[nextIndex]) : curves[nextIndex];
                chain.Add(nextCurve);
                currentPoint = nextCurve.EndPoint;
            }

            return chain;
        }

        private static int FindNextCurve(
            IReadOnlyList<IFinitePath> curves,
            bool[] used,
            PointXY point,
            out bool reverse)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                if (used[i])
                    continue;

                if (curves[i].StartPoint.AlmostEquals(point))
                {
                    reverse = false;
                    return i;
                }
            }

            for (int i = 0; i < curves.Count; i++)
            {
                if (used[i])
                    continue;

                if (curves[i].EndPoint.AlmostEquals(point))
                {
                    reverse = true;
                    return i;
                }
            }

            reverse = false;
            return -1;
        }

        private static IFinitePath ReverseCurve(IFinitePath curve)
        {
            if (curve is ParameterizedArc arc)
            {
                AngularDirection direction = arc.AngularDirection == AngularDirection.Counterclockwise
                    ? AngularDirection.Clockwise
                    : AngularDirection.Counterclockwise;

                return new ParameterizedArc(
                    arc.Center,
                    arc.Radius,
                    arc.EndAngle,
                    arc.StartAngle,
                    direction);
            }

            return new ParameterizedSegment(curve.EndPoint, curve.StartPoint, true, false);
        }

        private static float GetOffsetEpsilon(float offsetDistance)
        {
            return MathF.Max(GeometryConstants.GeometryEpsilon * 64f, offsetDistance * 1e-5f);
        }

        private static int GetPreviousIndex(int index, int count)
        {
            return index == 0 ? count - 1 : index - 1;
        }

        private readonly struct OffsetJoin
        {
            public OffsetJoin(
                PointXY previousEndPoint,
                PointXY nextStartPoint,
                ParameterizedArc? arc)
            {
                PreviousEndPoint = previousEndPoint;
                NextStartPoint = nextStartPoint;
                Arc = arc;
            }

            public PointXY PreviousEndPoint { get; }

            public PointXY NextStartPoint { get; }

            public ParameterizedArc? Arc { get; }
        }
    }
}
