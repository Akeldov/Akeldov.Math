using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-source culling selections into a 16-bit RGBA raster.
    /// </summary>
    /// <remarks>
    /// Each selected source is assigned a color from its position. For every raster cell, the configured
    /// culler selects relevant sources at the cell center and the rasterizer writes the linear RGB average
    /// color of the selected sources.
    /// </remarks>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    public sealed class CullingMapRGBA16BitRasterizer<TPointSource> :
        IRasterizer<IReadOnlyList<TPointSource>, Raster<RGBA16BitColor>>
        where TPointSource : IPointInfluenceSource
    {
        private const float SrgbLinearThreshold = 0.04045f;
        private const float LinearSrgbThreshold = 0.0031308f;

        private readonly IInfluenceSourceCuller<TPointSource> _culler;
        private readonly Func<PointXY, RGBA16BitColor> _sourcePositionToColor;

        /// <summary>
        /// Initializes a new culling map rasterizer with the specified source position color selector.
        /// </summary>
        /// <param name="culler">The culler used to select sources for each raster cell center.</param>
        /// <param name="sourcePositionToColor">The function that maps a selected source position to a 16-bit RGBA color.</param>
        public CullingMapRGBA16BitRasterizer(
            IInfluenceSourceCuller<TPointSource> culler,
            Func<PointXY, RGBA16BitColor> sourcePositionToColor)
        {
            _culler = culler ?? throw new ArgumentNullException(nameof(culler));
            _sourcePositionToColor = sourcePositionToColor ?? throw new ArgumentNullException(nameof(sourcePositionToColor));
        }

        /// <inheritdoc/>
        public Raster<RGBA16BitColor> Rasterize(IReadOnlyList<TPointSource> source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            TPointSource[] sources = CopySources(source);
            var values = new RGBA16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    IReadOnlyList<TPointSource> selectedSources = Cull(point);
                    values[y * grid.Resolution.X + x] = GetSelectionColor(sources, selectedSources);
                }
            }

            return new Raster<RGBA16BitColor>(grid, values);
        }

        private IReadOnlyList<TPointSource> Cull(PointXY point)
        {
            IReadOnlyList<TPointSource> selectedSources = _culler.Cull(point);

            if (selectedSources == null)
                throw new InvalidOperationException(
                    "Influence source culler returned null. Culler implementations must return a non-empty source list.");

            if (selectedSources.Count == 0)
                throw new InvalidOperationException(
                    "Influence source culler returned an empty source list. Culler implementations must return at least one source.");

            return selectedSources;
        }

        private RGBA16BitColor GetSelectionColor(
            IReadOnlyList<TPointSource> sources,
            IReadOnlyList<TPointSource> selectedSources)
        {
            float red = 0f;
            float green = 0f;
            float blue = 0f;
            ulong alpha = 0UL;

            for (int i = 0; i < selectedSources.Count; i++)
            {
                TPointSource selectedSource = selectedSources[i];
                if (selectedSource is null)
                    throw new InvalidOperationException("Influence source culler returned a source list containing null.");

                if (!ContainsSource(sources, selectedSource))
                    throw new InvalidOperationException(
                        "Influence source culler returned a source that is not present in the rasterized source list.");

                RGBA16BitColor color = _sourcePositionToColor(selectedSource.Position);
                red += Srgb16ToLinear(color.R);
                green += Srgb16ToLinear(color.G);
                blue += Srgb16ToLinear(color.B);
                alpha += color.A;
            }

            float inverseCount = 1f / selectedSources.Count;
            ulong halfCount = (ulong)selectedSources.Count / 2UL;
            return new RGBA16BitColor(
                LinearToSrgb16(red * inverseCount),
                LinearToSrgb16(green * inverseCount),
                LinearToSrgb16(blue * inverseCount),
                (ushort)((alpha + halfCount) / (ulong)selectedSources.Count));
        }

        private static float Srgb16ToLinear(ushort value)
        {
            float srgb = value / (float)ushort.MaxValue;
            return srgb <= SrgbLinearThreshold
                ? srgb / 12.92f
                : MathF.Pow((srgb + 0.055f) / 1.055f, 2.4f);
        }

        private static ushort LinearToSrgb16(float value)
        {
            if (value <= 0f)
                return 0;

            if (value >= 1f)
                return ushort.MaxValue;

            float srgb = value <= LinearSrgbThreshold
                ? value * 12.92f
                : 1.055f * MathF.Pow(value, 1f / 2.4f) - 0.055f;

            return (ushort)MathF.Round(srgb * ushort.MaxValue);
        }

        private static bool ContainsSource(IReadOnlyList<TPointSource> sources, TPointSource selectedSource)
        {
            var comparer = EqualityComparer<TPointSource>.Default;

            for (int i = 0; i < sources.Count; i++)
            {
                if (comparer.Equals(sources[i], selectedSource))
                    return true;
            }

            return false;
        }

        private static TPointSource[] CopySources(IReadOnlyList<TPointSource> source)
        {
            if (source.Count == 0)
                throw new ArgumentException("Culling map source collection must not be empty.", nameof(source));

            var copy = new TPointSource[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                TPointSource pointSource = source[i];
                if (pointSource is null)
                    throw new ArgumentException("Culling map source collection cannot contain null elements.", nameof(source));

                if (!PointXYValidation.IsFinite(pointSource.Position))
                    throw new ArgumentException("Culling map source positions must be finite.", nameof(source));

                copy[i] = pointSource;
            }

            return copy;
        }

        private static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }
    }
}
