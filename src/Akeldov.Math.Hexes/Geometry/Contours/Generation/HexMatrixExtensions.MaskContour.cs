using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System.Collections.Generic;
using System;

namespace Akeldov.Math.Hexes.Geometry.Contours
{
    public static partial class HexMatrixExtensions
    {
        public static Segment[] ToContour<TPolyhexGeometry>(this TPolyhexGeometry polyhexGeometry)
            where TPolyhexGeometry : IPolyhexGeometry
        {
            return polyhexGeometry.ToContour(Layout.OddR);
        }

        public static Segment[] ToContour<TPolyhexGeometry>(
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

            var borderLines = new List<Segment>();

            for (int q = 0; q < qsize; q++)
            {
                for (int r = 0; r < rsize; r++)
                {
                    if (!polyhexGeometry[q, r])
                        continue;

                    VectorXY[] points = Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertexes(q, r, hexApothem, hexRadius, layout);

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

            return borderLines.ToArray();
        }

        private static Segment CreateSegment(
            VectorXY endpointA,
            VectorXY endpointB,
            bool includesEndpointA,
            bool includesEndpointB)
        {
            return new Segment(
                (PointXY)endpointA,
                (PointXY)endpointB,
                includesEndpointA,
                includesEndpointB);
        }

        private static Segment CreateSegment(VectorXY endpointA, VectorXY endpointB)
        {
            return new Segment((PointXY)endpointA, (PointXY)endpointB);
        }
    }
}
