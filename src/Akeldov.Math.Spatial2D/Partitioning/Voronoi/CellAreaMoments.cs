using Akeldov.Math.Spatial2D.Contours;
using static System.Math;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    // Oriented triangles and circular segments add material on outer loops and subtract holes.
    internal sealed class CellAreaMoments
    {
        private readonly PointXY _origin;
        private double _momentX, _momentY;

        public CellAreaMoments(PointXY origin) => _origin = origin;

        public double Area { get; private set; }
        public PointXY Centroid => new PointXY((float)(_origin.X + _momentX / Area), (float)(_origin.Y + _momentY / Area));

        public void AddChord(PointXY start, PointXY end)
        {
            double ax = (double)start.X - _origin.X, ay = (double)start.Y - _origin.Y;
            double bx = (double)end.X - _origin.X, by = (double)end.Y - _origin.Y;
            double area = 0.5 * (ax * by - ay * bx);
            Area += area;
            _momentX += area * (ax + bx) / 3;
            _momentY += area * (ay + by) / 3;
        }

        public void AddDisk(Circle circle, bool keepInside)
        {
            double area = PI * circle.Radius * circle.Radius * (keepInside ? 1 : -1);
            Area += area;
            _momentX += area * ((double)circle.Center.X - _origin.X);
            _momentY += area * ((double)circle.Center.Y - _origin.Y);
        }

        public void AddCircularSegment(PointXY start, PointXY end, double sweep)
        {
            double dx = (double)end.X - start.X, dy = (double)end.Y - start.Y;
            double chord = Sqrt(dx * dx + dy * dy);
            double angle = Abs(sweep);
            double sine = Sin(0.5 * angle);
            double segmentArea, offset;
            if (angle < 0.01)
            {
                // Series for theta - sin(theta) and the centroid's distance from the chord.
                double square = angle * angle;
                double arcLength = chord * (angle / (2 * sine));
                segmentArea = arcLength * arcLength * angle / 12 * (1 - square / 20 + square * square / 840);
                offset = arcLength * angle / 20 * (1 - 13 * square / 840);
            }
            else
            {
                double areaFactor = angle - Sin(angle);
                segmentArea = chord * chord * areaFactor / (8 * sine * sine);
                offset = 0.5 * chord * (4 * sine * sine / (3 * areaFactor) - Cos(angle * 0.5) / sine);
            }

            double area = Sign(sweep) * segmentArea;
            double normalScale = Sign(sweep) * offset / chord;
            Area += area;
            _momentX += area * (((double)start.X - _origin.X) + 0.5 * dx + dy * normalScale);
            _momentY += area * (((double)start.Y - _origin.Y) + 0.5 * dy - dx * normalScale);
        }
    }
}
