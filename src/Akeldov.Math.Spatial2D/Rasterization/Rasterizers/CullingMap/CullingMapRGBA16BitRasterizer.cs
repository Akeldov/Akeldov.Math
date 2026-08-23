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
    /// index selects relevant sources at the cell center and the rasterizer writes the linear RGB average
    /// color of the selected sources.
    /// </remarks>
    /// <typeparam name="TPointSource">The point influence source type.</typeparam>
    public sealed class CullingMapRGBA16BitRasterizer<TPointSource> :
        ISpatialRasterizer<IInfluenceSourceIndex<TPointSource>, RGBA16BitColor>
        where TPointSource : IPointInfluenceSource
    {
        private const float SrgbLinearThreshold = 0.04045f;
        private const float LinearSrgbThreshold = 0.0031308f;

        private readonly Func<PointXY, RGBA16BitColor> _sourcePositionToColor;

        /// <summary>
        /// Initializes a new culling map rasterizer with the specified source position color selector.
        /// </summary>
        /// <param name="sourcePositionToColor">The function that maps a selected source position to a 16-bit RGBA color.</param>
        public CullingMapRGBA16BitRasterizer(Func<PointXY, RGBA16BitColor> sourcePositionToColor)
        {
            _sourcePositionToColor = sourcePositionToColor ?? throw new ArgumentNullException(nameof(sourcePositionToColor));
        }

        /// <inheritdoc/>
        public SpatialRaster<RGBA16BitColor> Rasterize(IInfluenceSourceIndex<TPointSource> source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RasterGeometryValidation.ValidateAndGetCellCount(grid, nameof(grid));
            IReadOnlyList<TPointSource> sources = source.Sources;
            if (sources == null)
                throw new ArgumentException("Influence source index must expose a non-null source snapshot.", nameof(source));

            if (sources.Count == 0)
                throw new ArgumentException("Influence source index must contain at least one source.", nameof(source));

            for (int i = 0; i < sources.Count; i++)
            {
                TPointSource pointSource = sources[i];
                if (pointSource is null)
                    throw new ArgumentException("Influence source index snapshot cannot contain null elements.", nameof(source));

                if (!PointXYValidation.IsFinite(pointSource.Position))
                    throw new ArgumentException("Influence source positions must be finite.", nameof(source));
            }

            var sampler = new CullingMapRasterSampler(this, source);
            return SpatialRasterizationCore<RGBA16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private readonly struct CullingMapRasterSampler : ISpatialRasterSampler<RGBA16BitColor>
        {
            private readonly CullingMapRGBA16BitRasterizer<TPointSource> _rasterizer;
            private readonly IInfluenceSourceIndex<TPointSource> _source;

            public CullingMapRasterSampler(
                CullingMapRGBA16BitRasterizer<TPointSource> rasterizer,
                IInfluenceSourceIndex<TPointSource> source)
            {
                _rasterizer = rasterizer;
                _source = source;
            }

            public RGBA16BitColor Sample(PointXY point)
            {
                List<TPointSource> selectedSources = SelectSources(_source, point);
                return _rasterizer.GetSelectionColor(selectedSources);
            }
        }

        private static List<TPointSource> SelectSources(IInfluenceSourceIndex<TPointSource> source, PointXY point)
        {
            List<TPointSource> selectedSources = source.SelectSources(point);

            if (selectedSources == null)
                throw new InvalidOperationException(
                    "Influence source index returned null. Index implementations must return a non-empty source list.");

            if (selectedSources.Count == 0)
                throw new InvalidOperationException(
                    "Influence source index returned an empty source list. Index implementations must select at least one source.");

            return selectedSources;
        }

        private RGBA16BitColor GetSelectionColor(List<TPointSource> selectedSources)
        {
            float red = 0f;
            float green = 0f;
            float blue = 0f;
            ulong alpha = 0UL;

            for (int i = 0; i < selectedSources.Count; i++)
            {
                TPointSource selectedSource = selectedSources[i];
                if (selectedSource is null)
                    throw new InvalidOperationException(
                        "Influence source index returned a source list containing null.");

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

    }
}
