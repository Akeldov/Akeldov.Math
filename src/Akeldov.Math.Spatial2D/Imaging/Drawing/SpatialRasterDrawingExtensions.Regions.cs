using System;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws the boundary of a region, including hole boundaries, with antialiasing and source-over compositing.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="region">The region in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units, centered on the boundary.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses the absolute signed distance with 4-by-4 coverage sampling. Each pixel is composited once, including overlapping strokes. Color channels are composited without gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster or region is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IRegion region, float width, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (color.A == 0)
                return;

            var geometry = raster.Geometry;
            float halfWidth = width * 0.5f;
            for (int y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            var point = new PointXY(
                                geometry.Origin.X + (x + (sx + 0.5f) / 4) * geometry.CellSize.X,
                                geometry.Origin.Y + (y + (sy + 0.5f) / 4) * geometry.CellSize.Y);
                            if (MathF.Abs(region.SignedDistance(point)) <= halfWidth)
                                covered++;
                        }
                    }
                    if (covered != 0)
                        raster[x, y] = Composite(raster[x, y], color, covered / 16f);
                }
            }
        }
    }
}
