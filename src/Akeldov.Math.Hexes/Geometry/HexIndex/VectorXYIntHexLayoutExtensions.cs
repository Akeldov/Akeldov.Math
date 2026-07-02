using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYIntExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexCenter(this VectorXYInt index, float hexApothem, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return index.GetHexCenter(hexApothem, hexRadius, GetOffsetOrigin(hexApothem, hexRadius, layout), layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexCenter(this VectorXYInt index, float hexApothem, float hexRadius, VectorXY origin, Layout layout)
        {
            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            if (!origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(origin), origin, "Hex origin components must be finite.");

            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        origin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? hexApothem : 0f),
                        origin.Y + 1.5f * hexRadius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        origin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? -hexApothem : 0f),
                        origin.Y + 1.5f * hexRadius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        origin.X + 1.5f * hexRadius * index.X,
                        origin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? hexApothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        origin.X + 1.5f * hexRadius * index.X,
                        origin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? -hexApothem : 0f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorXY GetOffsetOrigin(float hexApothem, float hexRadius, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(hexApothem, hexRadius);
                case Layout.EvenR:
                    return new VectorXY(3f * hexApothem, hexRadius);
                case Layout.OddQ:
                    return new VectorXY(hexRadius, hexApothem);
                case Layout.EvenQ:
                    return new VectorXY(hexRadius, 3f * hexApothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
