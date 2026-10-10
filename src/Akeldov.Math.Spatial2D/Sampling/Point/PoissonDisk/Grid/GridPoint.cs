using System.Runtime.InteropServices;

namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct GridPoint
    {
        public GridPoint(PointXY point, float minimalDistance)
        {
            Point = point;
            MinimalDistance = minimalDistance;
        }

        public PointXY Point { get; }

        public float MinimalDistance { get; }
    }
}
