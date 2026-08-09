using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Describes an axis-aligned rectangular raster sampling grid in two-dimensional space.
    /// </summary>
    public readonly struct RasterGeometry : IEquatable<RasterGeometry>
    {
        private readonly PointXY _origin;
        private readonly VectorXY _size;
        private readonly VectorXYInt _resolution;
        private readonly VectorXY _cellSize;

        /// <summary>
        /// Initializes a new raster grid.
        /// </summary>
        /// <param name="origin">The lower-left grid origin in world coordinates.</param>
        /// <param name="size">The grid size in world coordinates. Both components must be finite and positive.</param>
        /// <param name="resolution">The grid resolution in cells. Both components must be positive.</param>
        public RasterGeometry(PointXY origin, VectorXY size, VectorXYInt resolution)
        {
            if (float.IsNaN(origin.X) || float.IsInfinity(origin.X) ||
                float.IsNaN(origin.Y) || float.IsInfinity(origin.Y))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(origin),
                    origin,
                    "Raster grid origin coordinates must be finite.");
            }

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Raster grid size components must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution,
                    "Raster grid resolution components must be positive.");

            _origin = origin;
            _size = size;
            _resolution = resolution;
            _cellSize = new VectorXY(size.X / resolution.X, size.Y / resolution.Y);
        }

        /// <summary>
        /// Initializes a new raster grid from an origin, size, and minimum pixel density.
        /// </summary>
        /// <param name="origin">The lower-left grid origin in world coordinates.</param>
        /// <param name="size">The grid size in world coordinates. Both components must be finite and positive.</param>
        /// <param name="minimumPixelsPerUnit">
        /// The minimum number of pixels per world-space unit. The resolution is rounded up independently for each axis.
        /// </param>
        public RasterGeometry(PointXY origin, VectorXY size, int minimumPixelsPerUnit)
        {
            if (minimumPixelsPerUnit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPixelsPerUnit),
                    minimumPixelsPerUnit,
                    "Minimum pixels per unit must be positive.");
            }

            var bounds = new RasterGeometry(origin, size, VectorXYInt.One);
            double resolutionX = System.Math.Ceiling((double)bounds.Size.X * minimumPixelsPerUnit);
            double resolutionY = System.Math.Ceiling((double)bounds.Size.Y * minimumPixelsPerUnit);

            if (double.IsInfinity(resolutionX) || resolutionX > int.MaxValue ||
                double.IsInfinity(resolutionY) || resolutionY > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPixelsPerUnit),
                    minimumPixelsPerUnit,
                    "The resulting raster grid resolution must fit in a 32-bit signed integer on both axes.");
            }

            _origin = bounds.Origin;
            _size = bounds.Size;
            _resolution = new VectorXYInt((int)resolutionX, (int)resolutionY);
            _cellSize = new VectorXY(
                _size.X / _resolution.X,
                _size.Y / _resolution.Y);
        }

        /// <summary>
        /// Initializes a new raster grid from any two diagonally opposite corners.
        /// </summary>
        /// <param name="cornerA">The first corner in world coordinates.</param>
        /// <param name="cornerB">The diagonally opposite corner in world coordinates. Corner order does not matter.</param>
        /// <param name="resolution">The grid resolution in cells. Both components must be positive.</param>
        public RasterGeometry(PointXY cornerA, PointXY cornerB, VectorXYInt resolution)
        {
            if (float.IsNaN(cornerA.X) || float.IsInfinity(cornerA.X) ||
                float.IsNaN(cornerA.Y) || float.IsInfinity(cornerA.Y))
                throw new ArgumentOutOfRangeException(
                    nameof(cornerA),
                    cornerA,
                    "Raster grid corner coordinates must be finite.");

            if (float.IsNaN(cornerB.X) || float.IsInfinity(cornerB.X) ||
                float.IsNaN(cornerB.Y) || float.IsInfinity(cornerB.Y))
                throw new ArgumentOutOfRangeException(
                    nameof(cornerB),
                    cornerB,
                    "Raster grid corner coordinates must be finite.");

            float minX = MathF.Min(cornerA.X, cornerB.X);
            float minY = MathF.Min(cornerA.Y, cornerB.Y);
            float width = MathF.Abs(cornerB.X - cornerA.X);
            float height = MathF.Abs(cornerB.Y - cornerA.Y);

            if (width <= 0f || height <= 0f)
                throw new ArgumentException("Raster grid corners must define a rectangle with positive width and height.", nameof(cornerB));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution,
                    "Raster grid resolution components must be positive.");

            _origin = new PointXY(minX, minY);
            _size = new VectorXY(width, height);
            _resolution = resolution;
            _cellSize = new VectorXY(width / resolution.X, height / resolution.Y);
        }

        /// <summary>
        /// Initializes a new raster grid from any two diagonally opposite corners and a minimum pixel density.
        /// </summary>
        /// <param name="cornerA">The first corner in world coordinates.</param>
        /// <param name="cornerB">The diagonally opposite corner in world coordinates. Corner order does not matter.</param>
        /// <param name="minimumPixelsPerUnit">
        /// The minimum number of pixels per world-space unit. The resolution is rounded up independently for each axis.
        /// </param>
        public RasterGeometry(PointXY cornerA, PointXY cornerB, int minimumPixelsPerUnit)
        {
            if (minimumPixelsPerUnit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPixelsPerUnit),
                    minimumPixelsPerUnit,
                    "Minimum pixels per unit must be positive.");
            }

            var bounds = new RasterGeometry(cornerA, cornerB, VectorXYInt.One);
            double resolutionX = System.Math.Ceiling((double)bounds.Size.X * minimumPixelsPerUnit);
            double resolutionY = System.Math.Ceiling((double)bounds.Size.Y * minimumPixelsPerUnit);

            if (double.IsInfinity(resolutionX) || resolutionX > int.MaxValue ||
                double.IsInfinity(resolutionY) || resolutionY > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumPixelsPerUnit),
                    minimumPixelsPerUnit,
                    "The resulting raster grid resolution must fit in a 32-bit signed integer on both axes.");
            }

            _origin = bounds.Origin;
            _size = bounds.Size;
            _resolution = new VectorXYInt((int)resolutionX, (int)resolutionY);
            _cellSize = new VectorXY(
                _size.X / _resolution.X,
                _size.Y / _resolution.Y);
        }

        /// <summary>
        /// Gets the lower-left grid origin in world coordinates.
        /// </summary>
        public PointXY Origin => _origin;

        /// <summary>
        /// Gets the grid size in world coordinates.
        /// </summary>
        public VectorXY Size => _size;

        /// <summary>
        /// Gets the grid resolution in cells.
        /// </summary>
        public VectorXYInt Resolution => _resolution;

        /// <summary>
        /// Gets the size of one raster cell in world coordinates.
        /// </summary>
        public VectorXY CellSize => _cellSize;

        /// <summary>
        /// Returns the center point of the specified raster cell in world coordinates.
        /// </summary>
        /// <param name="x">The zero-based X cell index.</param>
        /// <param name="y">The zero-based Y cell index.</param>
        /// <returns>The cell center in world coordinates.</returns>
        public PointXY GetCellCenter(int x, int y)
        {
            return GetCellCenter(new VectorXYInt(x, y));
        }

        /// <summary>
        /// Returns the center point of the specified raster cell in world coordinates.
        /// </summary>
        /// <param name="index">The zero-based cell index.</param>
        /// <returns>The cell center in world coordinates.</returns>
        public PointXY GetCellCenter(VectorXYInt index)
        {
            if (index.X < 0 || index.Y < 0 || index.X >= Resolution.X || index.Y >= Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(index), "Raster grid index must be inside the grid resolution.");

            return new PointXY(
                Origin.X + (index.X + 0.5f) * CellSize.X,
                Origin.Y + (index.Y + 0.5f) * CellSize.Y);
        }

        /// <summary>
        /// Indicates whether this raster grid has the same origin, size, and resolution as another raster grid.
        /// </summary>
        /// <param name="other">The raster grid to compare with this raster grid.</param>
        /// <returns><see langword="true"/> if both raster grids are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RasterGeometry other) =>
            Origin.Equals(other.Origin) &&
            Size.Equals(other.Size) &&
            Resolution.Equals(other.Resolution);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RasterGeometry other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Origin, Size, Resolution);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "RasterGeometry(origin: {0}, size: {1}, resolution: {2})",
                Origin,
                Size,
                Resolution);

        /// <summary>
        /// Indicates whether two raster grids are equal.
        /// </summary>
        /// <param name="left">The first raster grid.</param>
        /// <param name="right">The second raster grid.</param>
        /// <returns><see langword="true"/> if the raster grids are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RasterGeometry left, RasterGeometry right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two raster grids are different.
        /// </summary>
        /// <param name="left">The first raster grid.</param>
        /// <param name="right">The second raster grid.</param>
        /// <returns><see langword="true"/> if the raster grids are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RasterGeometry left, RasterGeometry right) => !(left == right);
    }
}
