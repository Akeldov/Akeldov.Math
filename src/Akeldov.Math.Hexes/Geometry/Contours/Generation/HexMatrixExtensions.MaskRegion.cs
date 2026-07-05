using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using System.Collections.Generic;
using System;

namespace Akeldov.Math.Hexes.Geometry.Contours
{
    public static partial class HexMatrixExtensions
    {
        /// <summary>
        /// Creates a filled region for the occupied hex cells using the default <see cref="Layout.OddR"/> layout.
        /// </summary>
        /// <typeparam name="TPolyhexGeometry">The polyhex geometry type.</typeparam>
        /// <param name="polyhexGeometry">The polyhex geometry whose occupied cells define the region.</param>
        /// <returns>
        /// A contour-based region whose contours follow the boundary of the occupied cells. Regions with holes
        /// or multiple disconnected boundary chains are represented by multiple contours and the even-odd fill rule.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polyhexGeometry"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the polyhex contains no occupied cells.</exception>
        public static ContourBasedRegion ToRegion<TPolyhexGeometry>(this TPolyhexGeometry polyhexGeometry)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            return polyhexGeometry.ToRegion(Layout.OddR);
        }

        /// <summary>
        /// Creates a filled region for the occupied hex cells using the specified hex layout.
        /// </summary>
        /// <typeparam name="TPolyhexGeometry">The polyhex geometry type.</typeparam>
        /// <param name="polyhexGeometry">The polyhex geometry whose occupied cells define the region.</param>
        /// <param name="layout">The layout used to map hex indices to world-space boundary segments.</param>
        /// <returns>
        /// A contour-based region whose contours follow the boundary of the occupied cells. Regions with holes
        /// or multiple disconnected boundary chains are represented by multiple contours and the even-odd fill rule.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polyhexGeometry"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="layout"/> is not a defined layout.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the polyhex contains no occupied cells.</exception>
        public static ContourBasedRegion ToRegion<TPolyhexGeometry>(
            this TPolyhexGeometry polyhexGeometry,
            Layout layout)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            if (polyhexGeometry is null)
                throw new ArgumentNullException(nameof(polyhexGeometry));

            if (layout != Layout.OddR &&
                layout != Layout.EvenR &&
                layout != Layout.OddQ &&
                layout != Layout.EvenQ)
                throw new ArgumentOutOfRangeException(nameof(layout));

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

            return CreateContourBasedRegion(borderLines);
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

        private static ContourBasedRegion CreateContourBasedRegion(IReadOnlyList<ParameterizedSegment> segments)
        {
            if (segments.Count == 0)
                throw new InvalidOperationException("Polyhex contour must contain at least one boundary segment.");

            var contours = new List<IContour>();
            var used = new bool[segments.Count];

            for (int i = 0; i < segments.Count; i++)
            {
                if (used[i])
                    continue;

                contours.Add(new CompositeContour(BuildClosedSegmentChain(segments, used, i)));
            }

            return new ContourBasedRegion(contours);
        }

        private static List<IFinitePath> BuildClosedSegmentChain(
            IReadOnlyList<ParameterizedSegment> segments,
            bool[] used,
            int startIndex)
        {
            var orderedCurves = new List<IFinitePath>();

            used[startIndex] = true;
            orderedCurves.Add(segments[startIndex]);

            PointXY startPoint = segments[startIndex].StartPoint;
            PointXY currentPoint = segments[startIndex].EndPoint;

            while (!currentPoint.AlmostEquals(startPoint))
            {
                int nextIndex = FindSegmentStartingAt(segments, used, currentPoint);
                if (nextIndex < 0)
                    throw new InvalidOperationException("Polyhex contour boundary segments must form closed continuous chains.");

                used[nextIndex] = true;
                orderedCurves.Add(segments[nextIndex]);
                currentPoint = segments[nextIndex].EndPoint;
            }

            return orderedCurves;
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
