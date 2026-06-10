using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Describes an axis-aligned rectangular raster sampling grid in two-dimensional space.
    /// </summary>
    public readonly struct RasterGrid : IEquatable<RasterGrid>
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
        public RasterGrid(PointXY origin, VectorXY size, VectorXYInt resolution)
        {
            PointXYValidation.ThrowIfNotFinite(
                origin,
                nameof(origin),
                "Raster grid origin coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), "Raster grid size components must be finite and positive.");

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Raster grid resolution components must be positive.");

            _origin = origin;
            _size = size;
            _resolution = resolution;
            _cellSize = new VectorXY(size.X / resolution.X, size.Y / resolution.Y);
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
        public bool Equals(RasterGrid other) =>
            Origin.Equals(other.Origin) &&
            Size.Equals(other.Size) &&
            Resolution.Equals(other.Resolution);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RasterGrid other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Origin, Size, Resolution);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "RasterGrid(origin: {0}, size: {1}, resolution: {2})",
                Origin,
                Size,
                Resolution);

        /// <summary>
        /// Indicates whether two raster grids are equal.
        /// </summary>
        /// <param name="left">The first raster grid.</param>
        /// <param name="right">The second raster grid.</param>
        /// <returns><see langword="true"/> if the raster grids are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RasterGrid left, RasterGrid right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two raster grids are different.
        /// </summary>
        /// <param name="left">The first raster grid.</param>
        /// <param name="right">The second raster grid.</param>
        /// <returns><see langword="true"/> if the raster grids are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RasterGrid left, RasterGrid right) => !(left == right);
    }
}
