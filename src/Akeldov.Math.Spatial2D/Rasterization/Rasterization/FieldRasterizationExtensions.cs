using System;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a spatial field by sampling it at the center of each raster cell.
        /// </summary>
        /// <typeparam name="TFieldValue">The value type sampled from the field.</typeparam>
        /// <typeparam name="TRasterValue">The raster cell value type produced by the selector.</typeparam>
        /// <param name="field">The spatial field to sample.</param>
        /// <param name="grid">The raster geometry that describes the sampled region.</param>
        /// <param name="selector">The function that maps each sampled field value to a raster value.</param>
        /// <returns>
        /// A spatial raster whose value array is new, mutable, and owned by the caller.
        /// </returns>
        public static SpatialRaster<TRasterValue> Rasterize<TFieldValue, TRasterValue>(
            this IField<TFieldValue> field,
            RasterGeometry grid,
            Func<TFieldValue, TRasterValue> selector)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var sampler = new FieldSpatialRasterSampler<TFieldValue, TRasterValue>(field, selector);
            return SpatialRasterizationCore<TRasterValue>.Rasterize(grid, sampler, nameof(grid));
        }

        private readonly struct FieldSpatialRasterSampler<TFieldValue, TRasterValue> : ISpatialRasterSampler<TRasterValue>
        {
            private readonly IField<TFieldValue> _field;
            private readonly Func<TFieldValue, TRasterValue> _selector;

            public FieldSpatialRasterSampler(IField<TFieldValue> field, Func<TFieldValue, TRasterValue> selector)
            {
                _field = field;
                _selector = selector;
            }

            public TRasterValue Sample(PointXY point) => _selector(_field.Sample(point));
        }
    }
}
