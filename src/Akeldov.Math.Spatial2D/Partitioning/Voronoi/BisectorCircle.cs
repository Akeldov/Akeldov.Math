using System;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;
using static System.Math;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    // For light site L, heavy site H and weight ratio r, expand |p-L|² - r²|p-H|²
    // about their internal division point S. With u = p-S, a = 1-r² and g = r(H-L),
    // the boundary is F(u) = a|u|² + 2g·u = 0. The lighter site owns F <= 0.
    internal sealed class BisectorCircle
    {
        private readonly double _anchorX, _anchorY, _a, _gx, _gy, _gLength;

        public BisectorCircle(Site owner, Site other)
        {
            KeepInside = owner.Weight < other.Weight;
            var light = KeepInside ? owner : other;
            var heavy = KeepInside ? other : owner;
            double low = light.Weight, high = heavy.Weight;
            double ratio = low / high;
            _a = ((high - low) / high) * (1 + ratio);
            _gx = ratio * ((double)heavy.Position.X - light.Position.X);
            _gy = ratio * ((double)heavy.Position.Y - light.Position.Y);
            _gLength = Sqrt(_gx * _gx + _gy * _gy);
            _anchorX = (high * light.Position.X + low * heavy.Position.X) / (high + low);
            _anchorY = (high * light.Position.Y + low * heavy.Position.Y) / (high + low);
        }

        public bool KeepInside { get; }

        public double Evaluate(double x, double y)
        {
            double ux = x - _anchorX, uy = y - _anchorY;
            return _a * (ux * ux + uy * uy) + 2 * (_gx * ux + _gy * uy);
        }

        public (double Min, double Max) Range(Rectangle bounds)
        {
            // F is separable and convex. Find the minimum of each quadratic on its bounded interval.
            double nearX = MinimumOnAxis(bounds.Min.X, bounds.Max.X, _anchorX, _gx);
            double nearY = MinimumOnAxis(bounds.Min.Y, bounds.Max.Y, _anchorY, _gy);
            float farX = _a * (((double)bounds.Min.X + bounds.Max.X) * 0.5 - _anchorX) + _gx > 0
                ? bounds.Max.X : bounds.Min.X;
            float farY = _a * (((double)bounds.Min.Y + bounds.Max.Y) * 0.5 - _anchorY) + _gy > 0
                ? bounds.Max.Y : bounds.Min.Y;
            return (Evaluate(nearX, nearY), Evaluate(farX, farY));
        }

        private double MinimumOnAxis(double min, double max, double origin, double g)
        {
            if (_a * (min - origin) + g >= 0)
                return min;
            if (_a * (max - origin) + g <= 0)
                return max;
            return origin - g / _a;
        }

        public int Quadrant(double x, double y)
        {
            double nx = _a * (x - _anchorX) + _gx, ny = _a * (y - _anchorY) + _gy;
            if (ny > 0)
                return nx > 0 ? 0 : 1;
            if (ny < 0)
                return nx < 0 ? 2 : 3;
            return nx >= 0 ? 0 : 2;
        }

        public bool TryGetCardinal(int quadrant, Rectangle bounds, out double x, out double y)
        {
            // At an extremum the half-gradient is an axis vector of length |g|.
            // Rationalize |g| - |gx| (or |gy|) when the bisector is nearly axis-aligned.
            double dx = -_gx, dy = -_gy;
            if (quadrant == 0 || quadrant == 2)
            {
                int sign = quadrant == 0 ? 1 : -1;
                dx = sign * _gx > 0 ? sign * _gy * _gy / (_gLength + Abs(_gx)) : sign * _gLength - _gx;
            }
            else
            {
                int sign = quadrant == 1 ? 1 : -1;
                dy = sign * _gy > 0 ? sign * _gx * _gx / (_gLength + Abs(_gy)) : sign * _gLength - _gy;
            }

            x = y = 0;
            if (dx < _a * (bounds.Min.X - _anchorX) || dx > _a * (bounds.Max.X - _anchorX)
                || dy < _a * (bounds.Min.Y - _anchorY) || dy > _a * (bounds.Max.Y - _anchorY))
                return false;
            x = _anchorX + dx / _a;
            y = _anchorY + dy / _a;
            return true;
        }

        public double MinorSweep(double dx, double dy)
            => 2 * Asin(Min(1, 0.5 * Sqrt(dx * dx + dy * dy) * _a / _gLength));

        // A full circle that survived clipping is an actual bounded output contour.
        public Circle ToCircle() => new Circle(new PointXY((float)(_anchorX - _gx / _a), (float)(_anchorY - _gy / _a)),
            (float)(_gLength / _a));

        public void IntersectSegment(PointXY start, PointXY end, Action<double, double> add)
        {
            double dx = (double)end.X - start.X, dy = (double)end.Y - start.Y;
            double length = Sqrt(dx * dx + dy * dy);
            double ux = dx / length, uy = dy / length;

            // Anchor the line near S to retain both intersections of a tiny disk with a long side.
            // Clamp the anchor to the segment for a small clipping window far from the sites.
            double along = (_anchorX - start.X) * ux + (_anchorY - start.Y) * uy;
            double x, y;
            if (along <= 0 || along >= length)
            {
                var endpoint = along <= 0 ? start : end;
                x = endpoint.X;
                y = endpoint.Y;
            }
            else
            {
                double normal = (_anchorX - start.X) * -uy + (_anchorY - start.Y) * ux;
                x = dx == 0 ? start.X : _anchorX + normal * uy;
                y = dy == 0 ? start.Y : _anchorY - normal * ux;
            }

            bool useX = Abs(dx) >= Abs(dy);
            float first = useX ? start.X : start.Y, last = useX ? end.X : end.Y;
            IntersectLine(x, y, ux, uy, (px, py) =>
            {
                double coordinate = useX ? px : py;
                if (coordinate >= Min(first, last) && coordinate <= Max(first, last))
                    add(px, py);
            });
        }

        public void IntersectCircle(BisectorCircle other, Rectangle bounds, Action<double, double> add)
        {
            if (_gLength * other._a > other._gLength * _a)
            {
                other.IntersectCircle(this, bounds, add);
                return;
            }

            // other.a * F - a * other.F cancels the quadratic term in a common local frame.
            double x = Max(bounds.Min.X, Min(bounds.Max.X, _anchorX));
            double y = Max(bounds.Min.Y, Min(bounds.Max.Y, _anchorY));
            double nx = other._a * (_a * (other._anchorX - _anchorX) + _gx) - _a * other._gx;
            double ny = other._a * (_a * (other._anchorY - _anchorY) + _gy) - _a * other._gy;
            double length = Sqrt(nx * nx + ny * ny);
            if (length == 0)
                return;

            nx /= length;
            ny /= length;
            double offset = (other._a * Evaluate(x, y) - _a * other.Evaluate(x, y)) / (2 * length);
            IntersectLine(x - offset * nx, y - offset * ny, -ny, nx, (px, py) =>
            {
                if (px >= bounds.Min.X && px <= bounds.Max.X && py >= bounds.Min.Y && py <= bounds.Max.Y)
                    add(px, py);
            });
        }

        private void IntersectLine(double x, double y, double ux, double uy, Action<double, double> add)
        {
            // Substitution of p(t) = (x,y) + t*(ux,uy) gives a*t² + 2*b*t + c = 0.
            double b = (_a * (x - _anchorX) + _gx) * ux + (_a * (y - _anchorY) + _gy) * uy;
            double c = Evaluate(x, y);
            double discriminant = b * b - _a * c;
            if (discriminant < 0)
                return;

            double root = Sqrt(discriminant);
            double q = -b - (b < 0 ? -root : root);
            double t = q / _a;
            add(x + t * ux, y + t * uy);
            if (root > 0)
            {
                t = c / q;
                add(x + t * ux, y + t * uy);
            }
        }
    }
}
