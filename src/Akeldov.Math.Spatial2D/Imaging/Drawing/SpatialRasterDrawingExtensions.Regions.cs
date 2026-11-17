using System;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Fills a region with linear fading outside over one raster pixel diagonal.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="region">The region in world coordinates.</param>
        /// <param name="color">The fill color and opacity.</param>
        /// <remarks>The fading distance is the length of the cell diagonal in world units, including for non-square pixels.</remarks>
        /// <exception cref="ArgumentNullException">The raster or region is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The cell diagonal cannot be represented as a finite float.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IRegion region, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            var cellSize = raster.Geometry.CellSize;
            float fadingDistance = (float)System.Math.Sqrt((double)cellSize.X * cellSize.X + (double)cellSize.Y * cellSize.Y);
            raster.Draw(region, fadingDistance, color);
        }

        /// <summary>Fills a region with full color opacity inside and linear fading outside, using source-over compositing.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="region">The region in world coordinates.</param>
        /// <param name="fadingDistance">The finite, non-negative distance in world units over which opacity falls linearly to zero outside the region. Zero selects a hard boundary with antialiasing.</param>
        /// <param name="color">The fill color and opacity.</param>
        /// <remarks>Uses signed distance with 4-by-4 coverage sampling. Interior samples have full coverage; exterior samples, including holes, fade to transparency. Each pixel is composited once. Color channels are composited without gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster or region is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The fading distance is not finite and non-negative.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IRegion region, float fadingDistance, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (float.IsNaN(fadingDistance) || float.IsInfinity(fadingDistance) || fadingDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(fadingDistance));
            if (color.A == 0)
                return;

            var geometry = raster.Geometry;
            for (int y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++)
                {
                    float covered = 0f;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            var point = new PointXY(
                                geometry.Origin.X + (x + (sx + 0.5f) / 4) * geometry.CellSize.X,
                                geometry.Origin.Y + (y + (sy + 0.5f) / 4) * geometry.CellSize.Y);
                            float distance = region.SignedDistance(point);
                            if (distance <= 0f)
                                covered += 1f;
                            else if (distance < fadingDistance)
                                covered += 1f - distance / fadingDistance;
                        }
                    }
                    if (covered != 0)
                        raster[x, y] = Composite(raster[x, y], color, covered / 16f);
                }
            }
        }
    }
}
