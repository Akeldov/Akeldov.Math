using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal struct PointDistanceRasterSampler<TSource, TValue> : ISpatialRasterSampler<TValue>
        where TSource : IPointDistanceProvider
    {
        private TSource _source;
        private readonly Func<float, TValue> _distanceToValue;

        public PointDistanceRasterSampler(TSource source, Func<float, TValue> distanceToValue)
        {
            _source = source;
            _distanceToValue = distanceToValue;
        }

        public TValue Sample(PointXY point)
        {
            return _distanceToValue(_source.Distance(point));
        }
    }
}
