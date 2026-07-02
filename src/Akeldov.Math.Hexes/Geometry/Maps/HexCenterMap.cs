using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    public sealed class HexCenterMap : IHexMap<PointXY>
    {
        private readonly PointXY[] _values;

        public HexCenterMap(
            int width,
            int height,
            VectorXY origin,
            float apothem,
            Layout layout)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (!origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(origin), origin, "Hex field origin components must be finite.");

            if (!IsFiniteAndPositive(apothem))
                throw new ArgumentOutOfRangeException(nameof(apothem), apothem, "Hex apothem must be finite and positive.");

            var radius = apothem.ConvertHexApothemToRadius();
            var count = checked(width * height);

            Width = width;
            Height = height;
            Origin = origin;
            Apothem = apothem;
            Layout = layout;
            _values = new PointXY[count];

            switch (layout)
            {
                case Layout.OddR:
                    FillRowLayoutCenters(false, radius);
                    break;
                case Layout.EvenR:
                    FillRowLayoutCenters(true, radius);
                    break;
                case Layout.OddQ:
                    FillColumnLayoutCenters(false, radius);
                    break;
                case Layout.EvenQ:
                    FillColumnLayoutCenters(true, radius);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        public HexCenterMap(
            int width,
            int height,
            float radius,
            Layout layout)
            : this(width, height, GetDefaultOriginFromRadius(radius, layout), ConvertValidRadiusToApothem(radius), layout)
        {
        }

        public int Width { get; }

        public int Height { get; }

        public VectorXY Origin { get; }

        public float Apothem { get; }

        public Layout Layout { get; }

        public PointXY this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Width ||
                    index.Y < 0 || index.Y >= Height)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _values[GetFlatIndex(index)];
            }
        }

        public PointXY this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        private void FillRowLayoutCenters(bool evenRowsAreShifted, float radius)
        {
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;
                var rowIsShifted = ((y & 1) == 0) == evenRowsAreShifted;
                var xShift = GetShiftRelativeToOrigin(rowIsShifted, evenRowsAreShifted);
                var centerY = Origin.Y + 1.5f * radius * y;

                for (int x = 0; x < Width; x++)
                {
                    _values[rowStart + x] = new PointXY(
                        Origin.X + x * 2f * Apothem + xShift,
                        centerY);
                }
            }
        }

        private void FillColumnLayoutCenters(bool evenColumnsAreShifted, float radius)
        {
            for (int y = 0; y < Height; y++)
            {
                var rowStart = y * Width;
                var baseY = Origin.Y + y * 2f * Apothem;

                for (int x = 0; x < Width; x++)
                {
                    var columnIsShifted = ((x & 1) == 0) == evenColumnsAreShifted;
                    var yShift = GetShiftRelativeToOrigin(columnIsShifted, evenColumnsAreShifted);

                    _values[rowStart + x] = new PointXY(
                        Origin.X + 1.5f * radius * x,
                        baseY + yShift);
                }
            }
        }

        private float GetShiftRelativeToOrigin(bool indexIsShifted, bool originIndexIsShifted)
        {
            if (indexIsShifted == originIndexIsShifted)
                return 0f;

            return indexIsShifted ? Apothem : -Apothem;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Width + index.X;

        private static float ConvertValidRadiusToApothem(float radius)
        {
            ValidateRadius(radius);
            return radius.ConvertHexRadiusToApothem();
        }

        private static VectorXY GetDefaultOriginFromRadius(float radius, Layout layout)
        {
            ValidateRadius(radius);
            return GetDefaultOrigin(radius.ConvertHexRadiusToApothem(), radius, layout);
        }

        private static void ValidateRadius(float radius)
        {
            if (!IsFiniteAndPositive(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");
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

        private static bool IsFiniteAndPositive(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
