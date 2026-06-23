using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class SignedPointDistanceBasedLayer<TColor, TSource> : IGeometrySceneLayer<TColor>
        where TSource : ISignedPointDistanceProvider
    {
        private readonly IReadOnlyList<TSource> _sources;
        private readonly Func<float, TColor> _signedDistanceToColor;

        public SignedPointDistanceBasedLayer(
            IReadOnlyList<TSource> sources,
            Func<float, TColor> signedDistanceToColor)
        {
            _sources = sources;
            _signedDistanceToColor = signedDistanceToColor;
        }

        public TColor Sample(PointXY point)
        {
            var minDist = float.MaxValue;
            for (int i = 0; i < _sources.Count; i++)
            {
                var source = _sources[i];
                var dist = source.SignedDistance(point);

                if (dist < minDist)
                    minDist = dist;
            }

            return _signedDistanceToColor(minDist);
        }
    }
}
