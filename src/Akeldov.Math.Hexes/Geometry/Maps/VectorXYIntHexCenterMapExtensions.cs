using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYIntExtensions
    {
        public static HexCenterMap ToHexCenterMap(this VectorXYInt resolution, Layout layout, float apothem)
        {
            float radius = apothem.ConvertHexApothemToRadius();
            VectorXY origin = GetDefaultOrigin(apothem, radius, layout);

            return new HexCenterMap(resolution.X, resolution.Y, origin, apothem, layout);
        }

        public static HexCenterMap ToHexCenterMap(
            this VectorXYInt resolution,
            Layout layout,
            VectorXY origin,
            float apothem)
        {
            return new HexCenterMap(resolution.X, resolution.Y, origin, apothem, layout);
        }

        private static VectorXY GetDefaultOrigin(float apothem, float radius, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(apothem, radius);
                case Layout.EvenR:
                    return new VectorXY(3f * apothem, radius);
                case Layout.OddQ:
                    return new VectorXY(radius, apothem);
                case Layout.EvenQ:
                    return new VectorXY(radius, 3f * apothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
