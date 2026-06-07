using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        public static Triplet<float> BarycentricCoordinates(this PointXY p, VectorXY a, VectorXY b, VectorXY c)
        {
            VectorXY v0 = new VectorXY(b.X - a.X, b.Y - a.Y);
            VectorXY v1 = new VectorXY((c - a).X, (c - a).Y);
            VectorXY v2 = new VectorXY(p.X - a.X, p.Y - a.Y);

            float d00 = VectorXY.Dot(v0, v0);
            float d01 = VectorXY.Dot(v0, v1);
            float d11 = VectorXY.Dot(v1, v1);
            float d20 = VectorXY.Dot(v2, v0);
            float d21 = VectorXY.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            if (denom == 0)
                denom = 1;

            
            float wB = (d11 * d20 - d01 * d21) / denom;
            float wC = (d00 * d21 - d01 * d20) / denom;

            
            float wA = 1.0f - wB - wC;

            return new Triplet<float>(wA, wB, wC);
        }

        public static Pair<float> BarycentricCoordinates(this PointXY p, VectorXY a, VectorXY b)
        {
            VectorXY ab = b - a;
            VectorXY ap = new VectorXY(p.X - a.X, p.Y - a.Y);

            float denominator = VectorXY.Dot(ab, ab);

            if (denominator == 0f)
                return new Pair<float>(1f, 0f);

            float wB = VectorXY.Dot(ap, ab) / denominator;
            float wA = 1f - wB;

            return new Pair<float>(wA, wB);
        }
    }
}
