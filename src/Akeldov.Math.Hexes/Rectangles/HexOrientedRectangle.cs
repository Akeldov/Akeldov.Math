using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Rectangles
{
    /// <summary>
    /// Represents a HexOrientedRectangle value.
    /// </summary>
    public readonly struct HexOrientedRectangle
    {
        private readonly PointXY _center;
        private readonly VectorXY _size;
        private readonly SixfoldAngle _rotation;

        private readonly PointXY _bottomLeft;
        private readonly PointXY _bottomRight;
        private readonly PointXY _topLeft;
        private readonly PointXY _topRight;

        /// <summary>
        /// Initializes a new instance of the HexOrientedRectangle type.
        /// </summary>
        /// <param name="center">The center value.</param>
        /// <param name="size">The size value.</param>
        /// <param name="rotation">The rotation value.</param>
        public HexOrientedRectangle(PointXY center, VectorXY size, SixfoldAngle rotation)
        {
            if (float.IsNaN(center.X) || float.IsInfinity(center.X) ||
                float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                throw new ArgumentOutOfRangeException(nameof(center), center, "Rectangle center coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            _center = center;
            _size = size;
            _rotation = rotation;

            var (halfSizeX, halfSizeY) = size / 2;

            _bottomLeft = RotateAround(_center + new VectorXY(-halfSizeX, -halfSizeY), _center, rotation);
            _bottomRight = RotateAround(_center + new VectorXY(halfSizeX, -halfSizeY), _center, rotation);
            _topLeft = RotateAround(_center + new VectorXY(-halfSizeX, halfSizeY), _center, rotation);
            _topRight = RotateAround(_center + new VectorXY(halfSizeX, halfSizeY), _center, rotation);
        }

        /// <summary>
        /// Gets the Center value.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the Size value.
        /// </summary>
        public VectorXY Size => _size;

        /// <summary>
        /// Gets the Rotation value.
        /// </summary>
        public SixfoldAngle Rotation => _rotation;

        /// <summary>
        /// Gets the BottomLeft value.
        /// </summary>
        public PointXY BottomLeft => _bottomLeft;

        /// <summary>
        /// Gets the BottomRight value.
        /// </summary>
        public PointXY BottomRight => _bottomRight;

        /// <summary>
        /// Gets the TopLeft value.
        /// </summary>
        public PointXY TopLeft => _topLeft;

        /// <summary>
        /// Gets the TopRight value.
        /// </summary>
        public PointXY TopRight => _topRight;

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="point">The point value.</param>
        public VectorXY GetLocalCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates;
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="point">The point value.</param>
        public VectorXY GetLocalNormalizedCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates.HadamardDivide(Size);
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="isClamped">The isClamped value.</param>
        public VectorXY GetLocalCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            if (isClamped)
                localCoordinates = localCoordinates.Clamp(VectorXY.Zero, Size);
            return localCoordinates;
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="isClamped">The isClamped value.</param>
        public VectorXY GetLocalNormalizedCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            var normalizedLocalCoordinates = localCoordinates.HadamardDivide(Size);
            if (isClamped)
                normalizedLocalCoordinates = normalizedLocalCoordinates.Clamp(VectorXY.Zero, VectorXY.One);
            return normalizedLocalCoordinates;
        }

        /// <summary>
        /// Creates a value from the specified inputs.
        /// </summary>
        /// <param name="bottomLeftPoint">The bottomLeftPoint value.</param>
        /// <param name="size">The size value.</param>
        /// <param name="rotation">The rotation value.</param>
        public static HexOrientedRectangle CreateFromBottomLeftPoint(PointXY bottomLeftPoint, VectorXY size, SixfoldAngle rotation)
        {
            if (float.IsNaN(bottomLeftPoint.X) || float.IsInfinity(bottomLeftPoint.X) ||
                float.IsNaN(bottomLeftPoint.Y) || float.IsInfinity(bottomLeftPoint.Y))
                throw new ArgumentOutOfRangeException(nameof(bottomLeftPoint), bottomLeftPoint, "Rectangle bottom-left point coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            var center = RotateAround(bottomLeftPoint + size * 0.5f, bottomLeftPoint, rotation);
            return new HexOrientedRectangle(center, size, rotation);
        }

        private static PointXY RotateAround(PointXY point, PointXY pivot, SixfoldAngle rotation)
        {
            return pivot + (point - pivot).Rotate(rotation);
        }

    }
}
