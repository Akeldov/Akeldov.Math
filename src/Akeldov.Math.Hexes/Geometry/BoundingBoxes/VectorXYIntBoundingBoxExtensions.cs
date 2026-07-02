using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYIntExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY BoundingBox(this VectorXYInt dim, float hexApothem, float hexRadius, Layout layout)
        {
            if (dim.X < 0 || dim.Y < 0)
                throw new ArgumentOutOfRangeException(nameof(dim), dim, "Couldn't calculate bounding box for dimensions with negative components.");

            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return RowLayoutBoundingBox(dim, hexApothem, hexRadius);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return ColumnLayoutBoundingBox(dim, hexApothem, hexRadius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXY RowLayoutBoundingBox(VectorXYInt dim, float hexApothem, float hexRadius)
        {
            if (dim.X == 0 || dim.Y == 0)
                return new VectorXY(0, 0);

            var xMetricSize = hexApothem * 2f * dim.X + hexApothem * (dim.Y == 1 ? 0 : 1);
            var yMetricSize = hexRadius * 2f + hexRadius * 1.5f * (dim.Y - 1);
            return new VectorXY(xMetricSize, yMetricSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXY ColumnLayoutBoundingBox(VectorXYInt dim, float hexApothem, float hexRadius)
        {
            if (dim.X == 0 || dim.Y == 0)
                return new VectorXY(0, 0);

            var xMetricSize = hexRadius * 2f + hexRadius * 1.5f * (dim.X - 1);
            var yMetricSize = hexApothem * 2f * dim.Y + hexApothem * (dim.X == 1 ? 0 : 1);
            return new VectorXY(xMetricSize, yMetricSize);
        }
    }
}
