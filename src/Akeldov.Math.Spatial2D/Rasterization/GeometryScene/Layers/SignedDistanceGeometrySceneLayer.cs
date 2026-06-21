using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class SignedDistanceGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly ISignedPointDistanceProvider _source;
        private readonly Func<float, TColor> _signedDistanceToColor;

        public SignedDistanceGeometrySceneLayer(
            ISignedPointDistanceProvider source,
            Func<float, TColor> signedDistanceToColor)
        {
            _source = source;
            _signedDistanceToColor = signedDistanceToColor;
        }

        public TColor Sample(PointXY point)
        {
            return _signedDistanceToColor(_source.SignedDistance(point));
        }
    }
}
