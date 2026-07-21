using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Vectors.QRS;
using System;

namespace Akeldov.Math.Hexes.Rectangles
{
    /// <summary>
    /// Represents a rectangle rotated in 60-degree increments to align with hex-grid directions.
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
        /// Initializes a rectangle from its center, unrotated size, and sixfold rotation.
        /// </summary>
        /// <param name="center">The rectangle center in world coordinates.</param>
        /// <param name="size">The width and height before rotation.</param>
        /// <param name="rotation">The counterclockwise rotation in 60-degree steps.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the center is not finite, a size component is not finite and positive,
        /// or the rotation is not a defined sixfold angle.
        /// </exception>
        public HexOrientedRectangle(PointXY center, VectorXY size, SixfoldAngle rotation)
        {
            if (float.IsNaN(center.X) || float.IsInfinity(center.X) ||
                float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                throw new ArgumentOutOfRangeException(nameof(center), center, "Rectangle center coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            if ((uint)rotation > (uint)SixfoldAngle.Deg300)
                throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Rectangle rotation must be a defined sixfold angle.");

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
        /// Gets the rectangle center in world coordinates.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the width and height in the rectangle's local coordinate system.
        /// </summary>
        public VectorXY Size => _size;

        /// <summary>
        /// Gets the counterclockwise rotation in 60-degree steps.
        /// </summary>
        public SixfoldAngle Rotation => _rotation;

        /// <summary>
        /// Gets the world position of the local bottom-left corner.
        /// </summary>
        public PointXY BottomLeft => _bottomLeft;

        /// <summary>
        /// Gets the world position of the local bottom-right corner.
        /// </summary>
        public PointXY BottomRight => _bottomRight;

        /// <summary>
        /// Gets the world position of the local top-left corner.
        /// </summary>
        public PointXY TopLeft => _topLeft;

        /// <summary>
        /// Gets the world position of the local top-right corner.
        /// </summary>
        public PointXY TopRight => _topRight;

        /// <summary>
        /// Converts a world point to coordinates measured from the local bottom-left corner.
        /// </summary>
        /// <param name="point">The point in world coordinates.</param>
        public VectorXY GetLocalCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates;
        }

        /// <summary>
        /// Converts a world point to local coordinates normalized by the rectangle size.
        /// </summary>
        /// <param name="point">The point in world coordinates.</param>
        /// <returns>Coordinates where <c>(0, 0)</c> is the local bottom-left corner and <c>(1, 1)</c> is the local top-right corner.</returns>
        public VectorXY GetLocalNormalizedCoordinates(PointXY point)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            return localCoordinates.HadamardDivide(Size);
        }

        /// <summary>
        /// Converts a world point to coordinates measured from the local bottom-left corner.
        /// </summary>
        /// <param name="point">The point in world coordinates.</param>
        /// <param name="isClamped">Whether to clamp the result to the rectangle bounds.</param>
        public VectorXY GetLocalCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            if (isClamped)
                localCoordinates = localCoordinates.Clamp(VectorXY.Zero, Size);
            return localCoordinates;
        }

        /// <summary>
        /// Converts a world point to local coordinates normalized by the rectangle size.
        /// </summary>
        /// <param name="point">The point in world coordinates.</param>
        /// <param name="isClamped">Whether to clamp each result component to the range from 0 through 1.</param>
        public VectorXY GetLocalNormalizedCoordinates(PointXY point, bool isClamped)
        {
            var localCoordinates = (point - BottomLeft).Rotate(Rotation.Negate());
            var normalizedLocalCoordinates = localCoordinates.HadamardDivide(Size);
            if (isClamped)
                normalizedLocalCoordinates = normalizedLocalCoordinates.Clamp(VectorXY.Zero, VectorXY.One);
            return normalizedLocalCoordinates;
        }

        /// <summary>
        /// Creates a rectangle whose local bottom-left corner is fixed at the specified world point.
        /// </summary>
        /// <param name="bottomLeftPoint">The world position of the local bottom-left corner.</param>
        /// <param name="size">The width and height before rotation.</param>
        /// <param name="rotation">The counterclockwise rotation in 60-degree steps.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the bottom-left point is not finite, a size component is not finite and positive,
        /// or the rotation is not a defined sixfold angle.
        /// </exception>
        public static HexOrientedRectangle CreateFromBottomLeftPoint(PointXY bottomLeftPoint, VectorXY size, SixfoldAngle rotation)
        {
            if (float.IsNaN(bottomLeftPoint.X) || float.IsInfinity(bottomLeftPoint.X) ||
                float.IsNaN(bottomLeftPoint.Y) || float.IsInfinity(bottomLeftPoint.Y))
                throw new ArgumentOutOfRangeException(nameof(bottomLeftPoint), bottomLeftPoint, "Rectangle bottom-left point coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            if ((uint)rotation > (uint)SixfoldAngle.Deg300)
                throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Rectangle rotation must be a defined sixfold angle.");

            var center = RotateAround(bottomLeftPoint + size * 0.5f, bottomLeftPoint, rotation);
            return new HexOrientedRectangle(center, size, rotation);
        }

        private static PointXY RotateAround(PointXY point, PointXY pivot, SixfoldAngle rotation)
        {
            return pivot + (point - pivot).Rotate(rotation);
        }

    }
}
