using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class PointDistanceBasedLayer<TColor, TSource> : IGeometrySceneLayer<TColor>
        where TSource : IPointDistanceProvider
    {
        private readonly IReadOnlyList<TSource> _sources;
        private readonly Func<float, TColor> _distanceToColor;

        public PointDistanceBasedLayer(
            IReadOnlyList<TSource> sources,
            Func<float, TColor> distanceToColor)
        {
            _sources = sources;
            _distanceToColor = distanceToColor;
        }

        public TColor Sample(PointXY point)
        {
            var minDist = float.MaxValue;
            for (int i = 0; i < _sources.Count; i++)
            {
                var source = _sources[i];
                var dist = source.Distance(point);

                if (dist < minDist)
                    minDist = dist;
            }

            return _distanceToColor(minDist);
        }
    }
}
