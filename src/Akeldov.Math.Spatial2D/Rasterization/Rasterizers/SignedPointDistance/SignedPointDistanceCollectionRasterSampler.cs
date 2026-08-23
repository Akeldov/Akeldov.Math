using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct SignedPointDistanceCollectionRasterSampler<TValue> : ISpatialRasterSampler<TValue>
    {
        private readonly IReadOnlyList<ISignedPointDistanceProvider> _sources;
        private readonly Func<float, TValue> _signedDistanceToValue;

        public SignedPointDistanceCollectionRasterSampler(IReadOnlyList<ISignedPointDistanceProvider> sources, Func<float, TValue> signedDistanceToValue)
        {
            _sources = sources;
            _signedDistanceToValue = signedDistanceToValue;
        }

        public TValue Sample(PointXY point)
        {
            float minSignedDistance = float.MaxValue;

            for (int i = 0; i < _sources.Count; i++)
            {
                float signedDistance = _sources[i].SignedDistance(point);
                if (signedDistance < minSignedDistance)
                    minSignedDistance = signedDistance;
            }

            return _signedDistanceToValue(minSignedDistance);
        }
    }
}
