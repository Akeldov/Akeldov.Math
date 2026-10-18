using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides color conversions for <see cref="ChromaticBarycentricPartialTripletRaster"/>.
    /// </summary>
    public static class ChromaticBarycentricPartialTripletRasterExtensions
    {
        /// <summary>
        /// Converts chromatically ordered weights to an opaque 8-bit RGBA raster.
        /// </summary>
        /// <remarks>
        /// Red, green, and blue are the weights of chromatic indices 0, 1, and 2, divided by the sum of present weights.
        /// A sample without present weights gets the class color of the nearest map hex.
        /// </remarks>
        /// <param name="raster">The chromatic barycentric raster to convert.</param>
        /// <returns>A new raster with the same geometry as <paramref name="raster"/>, owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static SpatialRaster<RGBA8BitColor> ToExtrapolatedRGBA8BitRaster(
            this ChromaticBarycentricPartialTripletRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new RGBA8BitColor[checked(raster.Resolution.X * raster.Resolution.Y)];
            for (int index = 0, y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++, index++)
                {
                    (float red, float green, float blue) = GetExtrapolatedChannels(raster, x, y);
                    values[index] = RGBA8BitColor.FromNormalized(red, green, blue);
                }
            }

            return new SpatialRaster<RGBA8BitColor>(raster.Geometry, values);
        }

        /// <summary>
        /// Converts chromatically ordered weights to an opaque 16-bit RGBA raster.
        /// </summary>
        /// <remarks>
        /// Red, green, and blue are the weights of chromatic indices 0, 1, and 2, divided by the sum of present weights.
        /// A sample without present weights gets the class color of the nearest map hex.
        /// </remarks>
        /// <param name="raster">The chromatic barycentric raster to convert.</param>
        /// <returns>A new raster with the same geometry as <paramref name="raster"/>, owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="raster"/> is <see langword="null"/>.</exception>
        public static SpatialRaster<RGBA16BitColor> ToExtrapolatedRGBA16BitRaster(
            this ChromaticBarycentricPartialTripletRaster raster)
        {
            if (raster is null)
                throw new ArgumentNullException(nameof(raster));

            var values = new RGBA16BitColor[checked(raster.Resolution.X * raster.Resolution.Y)];
            for (int index = 0, y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++, index++)
                {
                    (float red, float green, float blue) = GetExtrapolatedChannels(raster, x, y);
                    values[index] = RGBA16BitColor.FromNormalized(red, green, blue);
                }
            }

            return new SpatialRaster<RGBA16BitColor>(raster.Geometry, values);
        }

        private static (float Red, float Green, float Blue) GetExtrapolatedChannels(
            ChromaticBarycentricPartialTripletRaster raster,
            int x,
            int y)
        {
            PartialChromaticTriplet<float> weights = raster[x, y];
            float red = weights.HasIndex0 ? weights.Index0 : 0f;
            float green = weights.HasIndex1 ? weights.Index1 : 0f;
            float blue = weights.HasIndex2 ? weights.Index2 : 0f;
            float sum = red + green + blue;
            if (sum > 0f)
                return (red / sum, green / sum, blue / sum);

            HexMapGeometry hexMapGeometry = raster.SourceHexMapGeometry;
            int chromaticClass = GetNearestMapHexIndex(raster.Geometry.GetCellCenter(x, y), hexMapGeometry)
                .GetChromaticClass(hexMapGeometry.Topology.Layout);

            return (
                chromaticClass == 0 ? 1f : 0f,
                chromaticClass == 1 ? 1f : 0f,
                chromaticClass == 2 ? 1f : 0f);
        }

        private static VectorXYInt GetNearestMapHexIndex(PointXY point, HexMapGeometry hexMapGeometry)
        {
            // Hex centers lie on parallel lines: rows for row layouts and columns for column layouts. Odd lines are
            // shifted by an apothem, so a farther line always has a nearer line with the same shift. The nearest map
            // hex is therefore on the nearest line clamped to the map or on one of its two neighbors, and within a
            // line it is the nearest clamped position.
            Layout layout = hexMapGeometry.Topology.Layout;
            bool isRowLayout = layout == Layout.OddR || layout == Layout.EvenR;
            float oddLineShift = layout == Layout.OddR || layout == Layout.OddQ
                ? hexMapGeometry.Apothem
                : -hexMapGeometry.Apothem;
            VectorXYInt resolution = hexMapGeometry.Topology.Resolution;
            int lineCount = isRowLayout ? resolution.Y : resolution.X;
            int lineLength = isRowLayout ? resolution.X : resolution.Y;
            float along = isRowLayout ? point.X - hexMapGeometry.Origin.X : point.Y - hexMapGeometry.Origin.Y;
            float across = isRowLayout ? point.Y - hexMapGeometry.Origin.Y : point.X - hexMapGeometry.Origin.X;
            int nearestLine = System.Math.Clamp(
                (int)MathF.Floor(across / (1.5f * hexMapGeometry.Radius) + 0.5f),
                0,
                lineCount - 1);

            VectorXYInt nearestIndex = default;
            float minSquaredDistance = float.MaxValue;
            for (int line = System.Math.Max(nearestLine - 1, 0); line <= System.Math.Min(nearestLine + 1, lineCount - 1); line++)
            {
                float shift = (line & 1) == 1 ? oddLineShift : 0f;
                int position = System.Math.Clamp(
                    (int)MathF.Floor((along - shift) / (2f * hexMapGeometry.Apothem) + 0.5f),
                    0,
                    lineLength - 1);
                VectorXYInt index = isRowLayout ? new VectorXYInt(position, line) : new VectorXYInt(line, position);
                VectorXY center = index.GetHexCenter(hexMapGeometry.Radius, hexMapGeometry.Origin, layout);
                float deltaX = point.X - center.X;
                float deltaY = point.Y - center.Y;
                float squaredDistance = deltaX * deltaX + deltaY * deltaY;
                if (squaredDistance < minSquaredDistance)
                {
                    minSquaredDistance = squaredDistance;
                    nearestIndex = index;
                }
            }

            return nearestIndex;
        }
    }
}
