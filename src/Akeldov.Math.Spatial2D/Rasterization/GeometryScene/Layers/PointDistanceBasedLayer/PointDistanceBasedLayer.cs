using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class PointDistanceBasedLayer<TColor, TSource> : IGeometrySceneLayer<TColor>
        where TSource : IPointDistanceProvider
    {
        private readonly TSource[] _sources;
        private readonly Func<float, TColor> _distanceToColor;

        public PointDistanceBasedLayer(
            IReadOnlyList<TSource> sources,
            Func<float, TColor> distanceToColor)
        {
            _sources = CopySources(sources);
            _distanceToColor = distanceToColor ?? throw new ArgumentNullException(nameof(distanceToColor));
        }

        public TColor Sample(PointXY point)
        {
            var minDist = float.MaxValue;
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                var dist = source.Distance(point);

                if (dist < minDist)
                    minDist = dist;
            }

            return _distanceToColor(minDist);
        }

        private static TSource[] CopySources(IReadOnlyList<TSource> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Distance provider collection must not be empty.", nameof(sources));

            var copy = new TSource[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                TSource source = sources[i];
                if (source is null)
                    throw new ArgumentException("Distance provider collection cannot contain null elements.", nameof(sources));

                copy[i] = source;
            }

            return copy;
        }
    }
}
