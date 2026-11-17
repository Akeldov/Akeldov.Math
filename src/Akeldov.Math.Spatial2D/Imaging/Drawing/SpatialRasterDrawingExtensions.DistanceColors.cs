using System;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws a figure using a distance-dependent color function.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="figure">The figure in world coordinates.</param>
        /// <param name="color">Unsigned distance in world units. Returns the sample color and opacity.</param>
        /// <remarks>Evaluates the function at 4-by-4 samples in every pixel, averages premultiplied colors, and composites once using source-over without gamma correction. Transparent samples contribute nothing. The function is not retained. Exceptions propagate and may leave the raster partially drawn.</remarks>
        /// <exception cref="ArgumentNullException">The raster, figure, or color function is null.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IPointDistanceProvider figure, Func<float, RGBA8BitColor> color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (figure == null)
                throw new ArgumentNullException(nameof(figure));
            if (color == null)
                throw new ArgumentNullException(nameof(color));
            DrawSampleColors(raster, point => color(figure.Distance(point)));
        }

        /// <summary>Draws a figure using a distance-dependent color function.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="figure">The figure in world coordinates.</param>
        /// <param name="color">Signed distance in world units; negative values conventionally indicate the interior. Returns the sample color and opacity.</param>
        /// <remarks>Evaluates the function at 4-by-4 samples in every pixel, averages premultiplied colors, and composites once using source-over without gamma correction. Transparent samples contribute nothing. The function is not retained. Exceptions propagate and may leave the raster partially drawn.</remarks>
        /// <exception cref="ArgumentNullException">The raster, figure, or color function is null.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ISignedPointDistanceProvider figure, Func<float, RGBA8BitColor> color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (figure == null)
                throw new ArgumentNullException(nameof(figure));
            if (color == null)
                throw new ArgumentNullException(nameof(color));
            DrawSampleColors(raster, point => color(figure.SignedDistance(point)));
        }

        /// <summary>Draws a figure using a distance-dependent color function.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="figure">The figure in world coordinates.</param>
        /// <param name="color">Unsigned distance followed by the nearest curve coordinate, both in world units. The coordinate is not normalized. Ties follow the curve's ProjectWithParameter implementation. Returns the sample color and opacity.</param>
        /// <remarks>Evaluates the function at 4-by-4 samples in every pixel, averages premultiplied colors, and composites once using source-over without gamma correction. Transparent samples contribute nothing. The function is not retained. Exceptions propagate and may leave the raster partially drawn.</remarks>
        /// <exception cref="ArgumentNullException">The raster, figure, or color function is null.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IParameterizedCurve figure, Func<float, float, RGBA8BitColor> color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (figure == null)
                throw new ArgumentNullException(nameof(figure));
            if (color == null)
                throw new ArgumentNullException(nameof(color));
            DrawSampleColors(raster, point => { var projection = figure.ProjectWithParameter(point); return color(projection.Distance, projection.CurveCoordinate); });
        }

        private static void DrawSampleColors(SpatialRaster<RGBA8BitColor> raster, Func<PointXY, RGBA8BitColor> sampleColor)
        {
            var geometry = raster.Geometry;
            for (int y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++)
                {
                    double alpha = 0, red = 0, green = 0, blue = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            var point = new PointXY(
                                geometry.Origin.X + (x + (sx + 0.5f) / 4) * geometry.CellSize.X,
                                geometry.Origin.Y + (y + (sy + 0.5f) / 4) * geometry.CellSize.Y);
                            var sample = sampleColor(point);
                            alpha += sample.A;
                            red += sample.R * sample.A;
                            green += sample.G * sample.A;
                            blue += sample.B * sample.A;
                        }
                    }
                    if (alpha == 0)
                        continue;
                    const double scale = 16 * 255;
                    var target = raster[x, y];
                    double targetWeight = target.A / 255d * (1 - alpha / scale);
                    double outputAlpha = alpha / scale + targetWeight;
                    raster[x, y] = new RGBA8BitColor(
                        (byte)System.Math.Round((red / scale + target.R * targetWeight) / outputAlpha),
                        (byte)System.Math.Round((green / scale + target.G * targetWeight) / outputAlpha),
                        (byte)System.Math.Round((blue / scale + target.B * targetWeight) / outputAlpha),
                        (byte)System.Math.Round(outputAlpha * 255));
                }
            }
        }
    }
}

