namespace Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk
{
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
