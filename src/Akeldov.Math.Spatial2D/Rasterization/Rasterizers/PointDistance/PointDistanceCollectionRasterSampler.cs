using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct PointDistanceCollectionRasterSampler<TSource, TValue> : ISpatialRasterSampler<TValue>
        where TSource : IPointDistanceProvider
    {
        private readonly IReadOnlyList<TSource> _sources;
        private readonly Func<float, TValue> _distanceToValue;

        public PointDistanceCollectionRasterSampler(IReadOnlyList<TSource> sources, Func<float, TValue> distanceToValue)
        {
            _sources = sources;
            _distanceToValue = distanceToValue;
        }

        public TValue Sample(PointXY point)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < _sources.Count; i++)
            {
                float distance = _sources[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return _distanceToValue(minDistance);
        }
    }
}
