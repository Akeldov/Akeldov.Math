using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Rectangles
{
    public readonly struct HexOrientedRectangle
    {
        private readonly PointXY _center;
        private readonly VectorXY _size;
        private readonly SixfoldAngle _rotation;

        private readonly PointXY _bottomLeft;
        private readonly PointXY _bottomRight;
        private readonly PointXY _topLeft;
        private readonly PointXY _topRight;

        public HexOrientedRectangle(PointXY center, VectorXY size, SixfoldAngle rotation)
        {
            if (!IsFinite(center))
                throw new ArgumentOutOfRangeException(nameof(center), center, "Rectangle center coordinates must be finite.");

            ThrowIfInvalidSize(size, nameof(size));

            _center = center;
            _size = size;
            _rotation = rotation;

            var (halfSizeX, halfSizeY) = size / 2;

            _bottomLeft = RotateAround(_center + new VectorXY(-halfSizeX, -halfSizeY), _center, rotation);
            _bottomRight = RotateAround(_center + new VectorXY(halfSizeX, -halfSizeY), _center, rotation);
            _topLeft = RotateAround(_center + new VectorXY(-halfSizeX, halfSizeY), _center, rotation);
            _topRight = RotateAround(_center + new VectorXY(halfSizeX, halfSizeY), _center, rotation);
        }

        public PointXY Center => _center;

        public VectorXY Size => _size;

        public SixfoldAngle Rotation => _rotation;

        public PointXY BottomLeft => _bottomLeft;

        public PointXY BottomRight => _bottomRight;

        public PointXY TopLeft => _topLeft;

        public PointXY TopRight => _topRight;

        public VectorXY GetLocalCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates;
        }

        public VectorXY GetLocalNormalizedCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates.HadamardDivide(Size);
        }

        public VectorXY GetLocalCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            if (isClamped)
                localCoordinates = localCoordinates.Clamp(VectorXY.Zero, Size);
            return localCoordinates;
        }

        public VectorXY GetLocalNormalizedCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            var normalizedLocalCoordinates = localCoordinates.HadamardDivide(Size);
            if (isClamped)
                normalizedLocalCoordinates = normalizedLocalCoordinates.Clamp(VectorXY.Zero, VectorXY.One);
            return normalizedLocalCoordinates;
        }

        public static HexOrientedRectangle CreateFromBottomLeftPoint(PointXY bottomLeftPoint, VectorXY size, SixfoldAngle rotation)
        {
            if (!IsFinite(bottomLeftPoint))
                throw new ArgumentOutOfRangeException(nameof(bottomLeftPoint), bottomLeftPoint, "Rectangle bottom-left point coordinates must be finite.");

            ThrowIfInvalidSize(size, nameof(size));

            var center = RotateAround(bottomLeftPoint + size * 0.5f, bottomLeftPoint, rotation);
            return new HexOrientedRectangle(center, size, rotation);
        }

        private static PointXY RotateAround(PointXY point, PointXY pivot, SixfoldAngle rotation)
        {
            return pivot + (point - pivot).Rotate(rotation);
        }

        private static bool IsFinite(PointXY point) =>
            !float.IsNaN(point.X) && !float.IsInfinity(point.X) &&
            !float.IsNaN(point.Y) && !float.IsInfinity(point.Y);

        private static void ThrowIfInvalidSize(VectorXY size, string parameterName)
        {
            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, size, "Rectangle size components must be finite and positive.");
        }
    }
}
