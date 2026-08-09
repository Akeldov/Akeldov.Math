using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    internal static partial class RandomExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY SelectRandomAroundPoint(this Random rnd, PointXY point, float minimalDistance)
        {
            float radius = minimalDistance * (1 + (float)rnd.NextDouble());
            float angle = (float)rnd.NextDouble() * 2 * MathF.PI;

            var x = point.X + radius * MathF.Cos(angle);
            var y = point.Y + radius * MathF.Sin(angle);

            return new PointXY(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY SelectRandomPoint(this Random rnd, VectorXY fieldSize)
        {
            var x = (float)rnd.NextDouble() * fieldSize.X;
            var y = (float)rnd.NextDouble() * fieldSize.Y;
            return new PointXY(x, y);
        }
    }
}
