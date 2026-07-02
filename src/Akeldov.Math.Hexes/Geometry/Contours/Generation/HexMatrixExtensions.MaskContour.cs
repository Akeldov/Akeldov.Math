using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System.Collections.Generic;
using System;

namespace Akeldov.Math.Hexes.Geometry.Contours
{
    public static partial class HexMatrixExtensions
    {
        public static CompositeContour ToContour<TPolyhexGeometry>(this TPolyhexGeometry polyhexGeometry)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            return polyhexGeometry.ToContour(Layout.OddR);
        }

        public static CompositeContour ToContour<TPolyhexGeometry>(
            this TPolyhexGeometry polyhexGeometry,
            Layout layout)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            if (polyhexGeometry is null)
                throw new ArgumentNullException(nameof(polyhexGeometry));

            var hexApothem = polyhexGeometry.HexApothem;
            var hexRadius = polyhexGeometry.HexRadius;
            int qsize = polyhexGeometry.QRSResolution.Q;
            int rsize = polyhexGeometry.QRSResolution.R;

            var borderLines = new List<ParameterizedSegment>();

            for (int q = 0; q < qsize; q++)
            {
                for (int r = 0; r < rsize; r++)
                {
                    if (!polyhexGeometry[q, r])
                        continue;

                    VectorXY[] points = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertices(q, r, hexApothem, hexRadius, layout);

                    var qminClause = q < 1;
                    var rminClause = r < 1;
                    var qmaxClause = q >= qsize - 1;
                    var rmaxClause = r >= rsize - 1;

                    var leftIsBorder = qminClause || !polyhexGeometry[q - 1, r];
                    var rightIsBorder = qmaxClause || !polyhexGeometry[q + 1, r];
                    var topLeftIsBorder = qminClause || rmaxClause || !polyhexGeometry[q - 1, r + 1];
                    var topRightIsBorder = rmaxClause || !polyhexGeometry[q, r + 1];
                    var bottomLeftIsBorder = rminClause || !polyhexGeometry[q, r - 1];
                    var bottomRightIsBorder = qmaxClause || rminClause || !polyhexGeometry[q + 1, r - 1];

                    if (layout == Layout.OddR || layout == Layout.EvenR)
                    {
                        if (leftIsBorder) borderLines.Add(CreateSegment(points[2], points[3], true, false));
                        if (rightIsBorder) borderLines.Add(CreateSegment(points[5], points[0], true, false));
                        if (topLeftIsBorder) borderLines.Add(CreateSegment(points[1], points[2], true, false));
                        if (topRightIsBorder) borderLines.Add(CreateSegment(points[0], points[1], true, false));
                        if (bottomLeftIsBorder) borderLines.Add(CreateSegment(points[3], points[4], true, false));
                        if (bottomRightIsBorder) borderLines.Add(CreateSegment(points[4], points[5], true, false));
                    }
                    else
                    {
                        if (leftIsBorder) borderLines.Add(CreateSegment(points[3], points[4], true, false));
                        if (rightIsBorder) borderLines.Add(CreateSegment(points[0], points[1], true, false));
                        if (topLeftIsBorder) borderLines.Add(CreateSegment(points[2], points[3], true, false));
                        if (topRightIsBorder) borderLines.Add(CreateSegment(points[1], points[2], true, false));
                        if (bottomLeftIsBorder) borderLines.Add(CreateSegment(points[4], points[5], true, false));
                        if (bottomRightIsBorder) borderLines.Add(CreateSegment(points[5], points[0], true, false));
                    }
                }
            }

            return CreateCompositeContour(borderLines);
        }

        private static ParameterizedSegment CreateSegment(
            VectorXY endpointA,
            VectorXY endpointB,
            bool includesEndpointA,
            bool includesEndpointB)
        {
            return new ParameterizedSegment(
                (PointXY)endpointA,
                (PointXY)endpointB,
                includesEndpointA,
                includesEndpointB);
        }

        private static ParameterizedSegment CreateSegment(VectorXY endpointA, VectorXY endpointB)
        {
            return new ParameterizedSegment((PointXY)endpointA, (PointXY)endpointB);
        }

        private static CompositeContour CreateCompositeContour(IReadOnlyList<ParameterizedSegment> segments)
        {
            if (segments.Count == 0)
                throw new InvalidOperationException("Polyhex contour must contain at least one boundary segment.");

            var orderedCurves = new List<IFinitePath>(segments.Count);
            var used = new bool[segments.Count];

            used[0] = true;
            orderedCurves.Add(segments[0]);

            PointXY startPoint = segments[0].StartPoint;
            PointXY currentPoint = segments[0].EndPoint;

            for (int i = 1; i < segments.Count; i++)
            {
                int nextIndex = FindSegmentStartingAt(segments, used, currentPoint);
                if (nextIndex < 0)
                    throw new InvalidOperationException("Polyhex contour must form a single closed continuous chain.");

                used[nextIndex] = true;
                orderedCurves.Add(segments[nextIndex]);
                currentPoint = segments[nextIndex].EndPoint;
            }

            if (!currentPoint.AlmostEquals(startPoint))
                throw new InvalidOperationException("Polyhex contour must form a closed continuous chain.");

            return new CompositeContour(orderedCurves);
        }

        private static int FindSegmentStartingAt(
            IReadOnlyList<ParameterizedSegment> segments,
            bool[] used,
            PointXY point)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if (!used[i] && segments[i].StartPoint.AlmostEquals(point))
                    return i;
            }

            return -1;
        }
    }
}
