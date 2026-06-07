using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class PointXYExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToQRS(this PointXY point, VectorXY newOrigin, Layout layout)
        {
            var shiftedPoint = point - newOrigin;

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorQRS(
                        0.5773502588f * shiftedPoint.X - 0.3333333333f * shiftedPoint.Y,
                        0.6666666666f * shiftedPoint.Y);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorQRS(
                        0.6666666666f * shiftedPoint.X,
                        0.5773502588f * shiftedPoint.Y - 0.3333333333f * shiftedPoint.X);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
