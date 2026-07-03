using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Rasterization
{
    public sealed class HexFieldGeometryRGBA16BitRasterizer :
        IRasterizer<HexCenterMap, Raster<RGBA16BitColor>>
    {
        private readonly Func<PointXY, RGBA16BitColor> _centerToColor;

        public HexFieldGeometryRGBA16BitRasterizer(Func<PointXY, RGBA16BitColor> centerToColor)
        {
            _centerToColor = centerToColor ?? throw new ArgumentNullException(nameof(centerToColor));
        }

        public Raster<RGBA16BitColor> Rasterize(HexCenterMap source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateSource(source);
            ValidateGrid(grid);

            float radius = source.Apothem.ConvertHexApothemToRadius();
            VectorXY[] normalizedVertices = Geometry.VectorXYExtensions.GetNormalizedHexVertices(source.Layout);
            var values = new RGBA16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
            int count = checked(source.Width * source.Height);

            for (int i = 0; i < count; i++)
            {
                PointXY center = source[i];
                RGBA16BitColor color = _centerToColor(center);
                RasterizeHex(center, radius, normalizedVertices, grid, values, color);
            }

            return new Raster<RGBA16BitColor>(grid, values);
        }

        public static RasterGrid CreateGrid(HexCenterMap source, float pixelsPerApothem)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (float.IsNaN(pixelsPerApothem) || float.IsInfinity(pixelsPerApothem) || pixelsPerApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerApothem));

            ValidateSource(source);

            float radius = source.Apothem.ConvertHexApothemToRadius();
            VectorXY[] normalizedVertices = Geometry.VectorXYExtensions.GetNormalizedHexVertices(source.Layout);
            RasterBounds bounds = GetBounds(source, radius, normalizedVertices);
            double pixelsPerWorldUnit = (double)pixelsPerApothem / source.Apothem;
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

        private static void RasterizeHex(
            PointXY center,
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
            PointXY center,
            float radius,
            VectorXY[] normalizedVertices,
            PointXY point)
        {
            VectorXY centerVector = (VectorXY)center;
            VectorXY pointVector = (VectorXY)point;

            for (int i = 0; i < normalizedVertices.Length; i++)
            {
                VectorXY vertexA = centerVector + normalizedVertices[i] * radius;
                VectorXY vertexB = centerVector + normalizedVertices[(i + 1) % normalizedVertices.Length] * radius;
                VectorXY edge = vertexB - vertexA;
                VectorXY toPoint = pointVector - vertexA;

                if (VectorXY.Cross(edge, toPoint) < -GeometryConstants.GeometryEpsilon)
                    return false;
            }

            return true;
        }

        private static RasterBounds GetBounds(
            HexCenterMap source,
            float radius,
            VectorXY[] normalizedVertices)
        {
            int count = checked(source.Width * source.Height);
            RasterBounds bounds = GetHexBounds(source[0], radius, normalizedVertices);

            for (int i = 1; i < count; i++)
            {
                bounds = bounds.Include(GetHexBounds(source[i], radius, normalizedVertices));
            }

            return bounds;
        }

        private static void ValidateSource(HexCenterMap source)
        {
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException("Hex field geometry must contain at least one hex.", nameof(source));

            if (float.IsNaN(source.Apothem) || float.IsInfinity(source.Apothem) || source.Apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(source), "Hex field apothem must be finite and positive.");
        }

        private static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }

        private static RasterBounds GetHexBounds(
            PointXY center,
            float radius,
            VectorXY[] normalizedVertices)
        {
            VectorXY centerVector = (VectorXY)center;
            VectorXY first = centerVector + normalizedVertices[0] * radius;
            float minX = first.X;
            float minY = first.Y;
            float maxX = first.X;
            float maxY = first.Y;

            for (int i = 1; i < normalizedVertices.Length; i++)
            {
                VectorXY vertex = centerVector + normalizedVertices[i] * radius;
                minX = MathF.Min(minX, vertex.X);
                minY = MathF.Min(minY, vertex.Y);
                maxX = MathF.Max(maxX, vertex.X);
                maxY = MathF.Max(maxY, vertex.Y);
            }

            return new RasterBounds(minX, minY, maxX, maxY);
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
