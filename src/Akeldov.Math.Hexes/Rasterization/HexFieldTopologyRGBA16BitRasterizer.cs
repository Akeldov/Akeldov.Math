using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Rasterization
{
    /// <summary>
    /// Initializes a new instance of the HexFieldTopologyRGBA16BitRasterizer type.
    /// </summary>
    public sealed class HexFieldTopologyRGBA16BitRasterizer :
        IRasterizer<IndexSeptupletMap, Raster<RGBA16BitColor>>
    {
        private const float ApothemToRadius = 1.1547005f;

        private static readonly VectorXY[] RowLayoutNormalizedHexVertices =
        {
            new VectorXY(0.8660254f, 0.5f),
            new VectorXY(0.0f, 1.0f),
            new VectorXY(-0.8660254f, 0.5f),
            new VectorXY(-0.8660254f, -0.5f),
            new VectorXY(0.0f, -1.0f),
            new VectorXY(0.8660254f, -0.5f)
        };

        private static readonly VectorXY[] ColumnLayoutNormalizedHexVertices =
        {
            new VectorXY(1.0f, 0.0f),
            new VectorXY(0.5f, 0.8660254f),
            new VectorXY(-0.5f, 0.8660254f),
            new VectorXY(-1.0f, 0.0f),
            new VectorXY(-0.5f, -0.8660254f),
            new VectorXY(0.5f, -0.8660254f)
        };

        private readonly VectorXY _origin;
        private readonly float _apothem;
        private readonly Func<VectorXYInt, RGBA16BitColor> _indexToColor;

        /// <summary>
        /// Initializes a new instance of the HexFieldTopologyRGBA16BitRasterizer type.
        /// </summary>
        /// <param name="origin">The Origin value.</param>
        /// <param name="apothem">The Apothem value.</param>
        /// <param name="indexToColor">The IndexToColor value.</param>
        public HexFieldTopologyRGBA16BitRasterizer(
            VectorXY origin,
            float apothem,
            Func<VectorXYInt, RGBA16BitColor> indexToColor)
        {
            if (!origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(origin), origin, "Hex field origin components must be finite.");

            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem));

            _origin = origin;
            _apothem = apothem;
            _indexToColor = indexToColor ?? throw new ArgumentNullException(nameof(indexToColor));
        }

        /// <summary>
        /// Rasterizes the specified hex-grid data.
        /// </summary>
        /// <param name="source">The source value.</param>
        /// <param name="grid">The grid value.</param>
        public Raster<RGBA16BitColor> Rasterize(IndexSeptupletMap source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateSource(source);
            ValidateGrid(grid);

            float radius = _apothem * ApothemToRadius;
            VectorXY[] normalizedVertices = GetNormalizedHexVertices(source.Layout);
            var values = new RGBA16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    VectorXY center = GetHexCenter(x, y, source.Layout, radius);
                    RGBA16BitColor color = _indexToColor(new VectorXYInt(x, y));
                    RasterizeHex(center, radius, normalizedVertices, grid, values, color);
                }
            }

            return new Raster<RGBA16BitColor>(grid, values);
        }

        /// <summary>
        /// Creates a value from the specified inputs.
        /// </summary>
        /// <param name="source">The source value.</param>
        /// <param name="pixelsPerApothem">The pixelsPerApothem value.</param>
        public RasterGrid CreateGrid(IndexSeptupletMap source, float pixelsPerApothem)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (float.IsNaN(pixelsPerApothem) || float.IsInfinity(pixelsPerApothem) || pixelsPerApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerApothem));

            ValidateSource(source);

            float radius = _apothem * ApothemToRadius;
            VectorXY[] normalizedVertices = GetNormalizedHexVertices(source.Layout);
            RasterBounds bounds = GetBounds(source, radius, normalizedVertices);
            double pixelsPerWorldUnit = (double)pixelsPerApothem / _apothem;
            int rasterWidth = CalculateRasterResolution(bounds.Width, pixelsPerWorldUnit);
            int rasterHeight = CalculateRasterResolution(bounds.Height, pixelsPerWorldUnit);

            return new RasterGrid(
                new PointXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                new VectorXYInt(rasterWidth, rasterHeight));
        }

        private static int CalculateRasterResolution(float worldSize, double pixelsPerWorldUnit)
        {
            double resolution = System.Math.Ceiling((double)worldSize * pixelsPerWorldUnit);
            if (double.IsNaN(resolution) || double.IsInfinity(resolution) || resolution > int.MaxValue)
                throw new OverflowException("Raster resolution must fit in Int32.");

            return resolution < 1d ? 1 : (int)resolution;
        }

        private VectorXY GetHexCenter(int x, int y, Layout layout, float radius)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        _origin.X + x * 2f * _apothem + ((y & 1) == 1 ? _apothem : 0f),
                        _origin.Y + 1.5f * radius * y);
                case Layout.EvenR:
                    return new VectorXY(
                        _origin.X + x * 2f * _apothem + ((y & 1) == 0 ? _apothem : 0f) - _apothem,
                        _origin.Y + 1.5f * radius * y);
                case Layout.OddQ:
                    return new VectorXY(
                        _origin.X + 1.5f * radius * x,
                        _origin.Y + y * 2f * _apothem + ((x & 1) == 1 ? _apothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        _origin.X + 1.5f * radius * x,
                        _origin.Y + y * 2f * _apothem + ((x & 1) == 0 ? _apothem : 0f) - _apothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorXY[] GetNormalizedHexVertices(Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return RowLayoutNormalizedHexVertices;
                case Layout.OddQ:
                case Layout.EvenQ:
                    return ColumnLayoutNormalizedHexVertices;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static void RasterizeHex(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertices,
            RasterGrid grid,
            RGBA16BitColor[] values,
            RGBA16BitColor color)
        {
            RasterBounds bounds = GetHexBounds(center, radius, normalizedVertices);
            int minX = System.Math.Max(0, (int)MathF.Floor((bounds.MinX - grid.Origin.X) / grid.CellSize.X));
            int maxX = System.Math.Min(grid.Resolution.X - 1, (int)MathF.Ceiling((bounds.MaxX - grid.Origin.X) / grid.CellSize.X) - 1);
            int minY = System.Math.Max(0, (int)MathF.Floor((bounds.MinY - grid.Origin.Y) / grid.CellSize.Y));
            int maxY = System.Math.Min(grid.Resolution.Y - 1, (int)MathF.Ceiling((bounds.MaxY - grid.Origin.Y) / grid.CellSize.Y) - 1);

            for (int y = minY; y <= maxY; y++)
            {
                float pointY = grid.Origin.Y + (y + 0.5f) * grid.CellSize.Y;

                for (int x = minX; x <= maxX; x++)
                {
                    PointXY point = new PointXY(grid.Origin.X + (x + 0.5f) * grid.CellSize.X, pointY);

                    if (ContainsPoint(center, radius, normalizedVertices, point))
                        values[y * grid.Resolution.X + x] = color;
                }
            }
        }

        private static bool ContainsPoint(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertices,
            PointXY point)
        {
            VectorXY pointVector = (VectorXY)point;

            for (int i = 0; i < normalizedVertices.Length; i++)
            {
                VectorXY vertexA = center + normalizedVertices[i] * radius;
                VectorXY vertexB = center + normalizedVertices[(i + 1) % normalizedVertices.Length] * radius;
                VectorXY edge = vertexB - vertexA;
                VectorXY toPoint = pointVector - vertexA;

                if (VectorXY.Cross(edge, toPoint) < -GeometryConstants.GeometryEpsilon)
                    return false;
            }

            return true;
        }

        private RasterBounds GetBounds(
            IndexSeptupletMap source,
            float radius,
            VectorXY[] normalizedVertices)
        {
            RasterBounds bounds = GetHexBounds(GetHexCenter(0, 0, source.Layout, radius), radius, normalizedVertices);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    bounds = bounds.Include(GetHexBounds(GetHexCenter(x, y, source.Layout, radius), radius, normalizedVertices));
                }
            }

            return bounds;
        }

        private static RasterBounds GetHexBounds(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertices)
        {
            VectorXY first = center + normalizedVertices[0] * radius;
            float minX = first.X;
            float minY = first.Y;
            float maxX = first.X;
            float maxY = first.Y;

            for (int i = 1; i < normalizedVertices.Length; i++)
            {
                VectorXY vertex = center + normalizedVertices[i] * radius;
                minX = MathF.Min(minX, vertex.X);
                minY = MathF.Min(minY, vertex.Y);
                maxX = MathF.Max(maxX, vertex.X);
                maxY = MathF.Max(maxY, vertex.Y);
            }

            return new RasterBounds(minX, minY, maxX, maxY);
        }

        private static void ValidateSource(IndexSeptupletMap source)
        {
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException("Hex field topology must contain at least one hex.", nameof(source));

            int expectedCount = checked(source.Width * source.Height);
            if (source.Count != expectedCount)
            {
                throw new ArgumentException("Hex adjacency map array length must match its dimensions.", nameof(source));
            }
        }

        private static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }

        private readonly struct RasterBounds
        {
            public RasterBounds(float minX, float minY, float maxX, float maxY)
            {
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
            }

            public float MinX { get; }

            public float MinY { get; }

            public float MaxX { get; }

            public float MaxY { get; }

            public float Width => MaxX - MinX;

            public float Height => MaxY - MinY;

            public RasterBounds Include(RasterBounds bounds)
            {
                return new RasterBounds(
                    MathF.Min(MinX, bounds.MinX),
                    MathF.Min(MinY, bounds.MinY),
                    MathF.Max(MaxX, bounds.MaxX),
                    MathF.Max(MaxY, bounds.MaxY));
            }
        }
    }
}
