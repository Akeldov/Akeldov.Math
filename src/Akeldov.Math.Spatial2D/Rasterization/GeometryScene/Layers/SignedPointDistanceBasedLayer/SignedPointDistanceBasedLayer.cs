using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class SignedPointDistanceBasedLayer<TColor, TSource> : IGeometrySceneLayer<TColor>
        where TSource : ISignedPointDistanceProvider
    {
        private readonly TSource[] _sources;
        private readonly Func<float, TColor> _signedDistanceToColor;

        public SignedPointDistanceBasedLayer(
            IReadOnlyList<TSource> sources,
            Func<float, TColor> signedDistanceToColor)
        {
            _sources = CopySources(sources);
            _signedDistanceToColor = signedDistanceToColor ?? throw new ArgumentNullException(nameof(signedDistanceToColor));
        }

        public TColor Sample(PointXY point)
        {
            var minDist = float.MaxValue;
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                var dist = source.SignedDistance(point);

                if (dist < minDist)
                    minDist = dist;
            }

            return _signedDistanceToColor(minDist);
        }

        private static TSource[] CopySources(IReadOnlyList<TSource> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Signed distance provider collection must not be empty.", nameof(sources));

            var copy = new TSource[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                TSource source = sources[i];
                if (source is null)
                    throw new ArgumentException("Signed distance provider collection cannot contain null elements.", nameof(sources));

                copy[i] = source;
            }

            return copy;
        }
    }
}
