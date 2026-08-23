using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct SignedPointDistanceRasterSampler<TValue> : ISpatialRasterSampler<TValue>
    {
        private readonly ISignedPointDistanceProvider _source;
        private readonly Func<float, TValue> _signedDistanceToValue;

        public SignedPointDistanceRasterSampler(ISignedPointDistanceProvider source, Func<float, TValue> signedDistanceToValue)
        {
            _source = source;
            _signedDistanceToValue = signedDistanceToValue;
        }

        public TValue Sample(PointXY point)
        {
            return _signedDistanceToValue(_source.SignedDistance(point));
        }
    }
}
