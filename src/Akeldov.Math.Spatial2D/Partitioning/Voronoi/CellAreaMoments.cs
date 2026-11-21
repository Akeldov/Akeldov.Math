using System;
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

        /// <param name="start">Start of the oriented chord.</param>
        /// <param name="end">End of the oriented chord.</param>
        /// <param name="sweep">Signed arc sweep in radians, at most a quarter turn in magnitude.</param>
        public void AddCircularSegment(PointXY start, PointXY end, double sweep)
        {
            // For chord length c and sweep magnitude t, the area is c² t * areaFactor
            // and the centroid's distance from the chord is c t * offsetFactor.
            float angle = (float)Abs(sweep);
            float areaFactor, offsetFactor;
            if (angle < 0.75f)
            {
                // Taylor expansions of the two factors avoid cancellation for shallow arcs.
                float square = angle * angle;
                areaFactor = 1f / 12 + square * (1f / 360 + square * (1f / 10080 + square / 302400));
                offsetFactor = 1f / 20 + square * (11f / 8400 + square * (1f / 28000 + square * (701f / 776160000)));
            }
            else
            {
                float sine = MathF.Sin(angle * 0.5f);
                float sineSquared = sine * sine;
                float difference = angle - MathF.Sin(angle);
                areaFactor = difference / (8 * sineSquared * angle);
                offsetFactor = (4 * sineSquared / (3 * difference) - MathF.Cos(angle * 0.5f) / sine) / (2 * angle);
            }

            double dx = (double)end.X - start.X, dy = (double)end.Y - start.Y;
            double area = (dx * dx + dy * dy) * sweep * areaFactor;
            double normalScale = sweep * offsetFactor;
            Area += area;
            _momentX += area * (((double)start.X - _origin.X) + 0.5 * dx + dy * normalScale);
            _momentY += area * (((double)start.Y - _origin.Y) + 0.5 * dy - dx * normalScale);
        }
    }
}
