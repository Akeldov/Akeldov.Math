using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Rasterization
{
    public sealed class HexFieldChromatizationRGBA16BitRasterizer :
        IRasterizer<ChromaticIndexMap, Raster<RGBA16BitColor>>
    {
        private const float ApothemToRadius = 1.1547005f;

        private static readonly VectorXY[] RowLayoutNormalizedHexVertexes =
        {
            new VectorXY(0.8660254f, 0.5f),
            new VectorXY(0.0f, 1.0f),
            new VectorXY(-0.8660254f, 0.5f),
            new VectorXY(-0.8660254f, -0.5f),
            new VectorXY(0.0f, -1.0f),
            new VectorXY(0.8660254f, -0.5f)
        };

        private static readonly VectorXY[] ColumnLayoutNormalizedHexVertexes =
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
        private readonly Func<byte, RGBA16BitColor> _chromaticIndexToColor;

        public HexFieldChromatizationRGBA16BitRasterizer(
            VectorXY origin,
            float apothem,
            Func<byte, RGBA16BitColor> chromaticIndexToColor)
        {
            if (float.IsNaN(apothem) || float.IsInfinity(apothem) || apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(apothem));

            _origin = origin;
            _apothem = apothem;
            _chromaticIndexToColor = chromaticIndexToColor ?? throw new ArgumentNullException(nameof(chromaticIndexToColor));
        }

        public Raster<RGBA16BitColor> Rasterize(ChromaticIndexMap source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateSource(source);
            ValidateGrid(grid);

            float radius = _apothem * ApothemToRadius;
            VectorXY[] normalizedVertexes = GetNormalizedHexVertexes(source.Layout);
            var values = new RGBA16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
            byte[] chromaticIndices = source.ChromaticIndexStorage;

            for (int y = 0; y < source.Height; y++)
            {
                int rowStart = y * source.Width;

                for (int x = 0; x < source.Width; x++)
                {
                    int index = rowStart + x;
                    VectorXY center = GetHexCenter(x, y, source.Layout, radius);
                    RGBA16BitColor color = _chromaticIndexToColor(chromaticIndices[index]);
                    RasterizeHex(center, radius, normalizedVertexes, grid, values, color);
                }
            }

            return new Raster<RGBA16BitColor>(grid, values);
        }

        public RasterGrid CreateGrid(ChromaticIndexMap source, float pixelsPerApothem)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (float.IsNaN(pixelsPerApothem) || float.IsInfinity(pixelsPerApothem) || pixelsPerApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerApothem));

            ValidateSource(source);

            float radius = _apothem * ApothemToRadius;
            VectorXY[] normalizedVertexes = GetNormalizedHexVertexes(source.Layout);
            RasterBounds bounds = GetBounds(source, radius, normalizedVertexes);
            float pixelsPerWorldUnit = pixelsPerApothem / _apothem;
            int rasterWidth = System.Math.Max(1, (int)MathF.Ceiling(bounds.Width * pixelsPerWorldUnit));
            int rasterHeight = System.Math.Max(1, (int)MathF.Ceiling(bounds.Height * pixelsPerWorldUnit));

            return new RasterGrid(
                new PointXY(bounds.MinX, bounds.MinY),
                new VectorXY(bounds.Width, bounds.Height),
                new VectorXYInt(rasterWidth, rasterHeight));
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

        private static VectorXY[] GetNormalizedHexVertexes(Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return RowLayoutNormalizedHexVertexes;
                case Layout.OddQ:
                case Layout.EvenQ:
                    return ColumnLayoutNormalizedHexVertexes;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static void RasterizeHex(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertexes,
            RasterGrid grid,
            RGBA16BitColor[] values,
            RGBA16BitColor color)
        {
            RasterBounds bounds = GetHexBounds(center, radius, normalizedVertexes);
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

                    if (ContainsPoint(center, radius, normalizedVertexes, point))
                        values[y * grid.Resolution.X + x] = color;
                }
            }
        }

        private static bool ContainsPoint(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertexes,
            PointXY point)
        {
            VectorXY pointVector = (VectorXY)point;

            for (int i = 0; i < normalizedVertexes.Length; i++)
            {
                VectorXY vertexA = center + normalizedVertexes[i] * radius;
                VectorXY vertexB = center + normalizedVertexes[(i + 1) % normalizedVertexes.Length] * radius;
                VectorXY edge = vertexB - vertexA;
                VectorXY toPoint = pointVector - vertexA;

                if (VectorXY.Cross(edge, toPoint) < -GeometryConstants.GeometryEpsilon)
                    return false;
            }

            return true;
        }

        private RasterBounds GetBounds(
            ChromaticIndexMap source,
            float radius,
            VectorXY[] normalizedVertexes)
        {
            RasterBounds bounds = GetHexBounds(GetHexCenter(0, 0, source.Layout, radius), radius, normalizedVertexes);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    bounds = bounds.Include(GetHexBounds(GetHexCenter(x, y, source.Layout, radius), radius, normalizedVertexes));
                }
            }

            return bounds;
        }

        private static RasterBounds GetHexBounds(
            VectorXY center,
            float radius,
            VectorXY[] normalizedVertexes)
        {
            VectorXY first = center + normalizedVertexes[0] * radius;
            float minX = first.X;
            float minY = first.Y;
            float maxX = first.X;
            float maxY = first.Y;

            for (int i = 1; i < normalizedVertexes.Length; i++)
            {
                VectorXY vertex = center + normalizedVertexes[i] * radius;
                minX = MathF.Min(minX, vertex.X);
                minY = MathF.Min(minY, vertex.Y);
                maxX = MathF.Max(maxX, vertex.X);
                maxY = MathF.Max(maxY, vertex.Y);
            }

            return new RasterBounds(minX, minY, maxX, maxY);
        }

        private static void ValidateSource(ChromaticIndexMap source)
        {
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException("Hex field chromatization must contain at least one hex.", nameof(source));

            int expectedCount = checked(source.Width * source.Height);
            if (source.ChromaticIndexStorage.Length != expectedCount)
                throw new ArgumentException("Hex field chromatization index count must match its dimensions.", nameof(source));
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
