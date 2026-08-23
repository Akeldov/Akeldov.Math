using System;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes floating-point influence fields into 16-bit RGBA rasters using a heat map color scale.
    /// </summary>
    public sealed class FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer :
        ISpatialRasterizer<FloatPointInfluenceField, RGBA16BitColor>
    {
        /// <inheritdoc/>
        public SpatialRaster<RGBA16BitColor> Rasterize(FloatPointInfluenceField source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RasterGeometryValidation.ValidateAndGetCellCount(grid, nameof(grid));
            ValidateRange(source);

            var sampler = new HeatMapRasterSampler(source);
            return SpatialRasterizationCore<RGBA16BitColor>.Rasterize(grid, sampler, nameof(grid));
        }

        private readonly struct HeatMapRasterSampler : ISpatialRasterSampler<RGBA16BitColor>
        {
            private readonly FloatPointInfluenceField _source;

            public HeatMapRasterSampler(FloatPointInfluenceField source)
            {
                _source = source;
            }

            public RGBA16BitColor Sample(PointXY point)
            {
                float value = _source.Sample(point);
                return RGBA16BitColor.FromTemperature(value, _source.Min, _source.Max);
            }
        }

        private static void ValidateRange(FloatPointInfluenceField source)
        {
            if (float.IsNaN(source.Min) || float.IsInfinity(source.Min) ||
                float.IsNaN(source.Max) || float.IsInfinity(source.Max) ||
                source.Max < source.Min)
            {
                throw new ArgumentException("Influence field range must be finite and ordered.", nameof(source));
            }
        }

    }
}
