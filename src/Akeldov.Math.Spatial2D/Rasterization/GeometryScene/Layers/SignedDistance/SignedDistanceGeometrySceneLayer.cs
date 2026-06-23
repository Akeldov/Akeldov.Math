using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class SignedDistanceGeometrySceneLayer<TColor> : GeometrySceneLayer<TColor>
    {
        private readonly ISignedPointDistanceProvider _source;
        private readonly Func<float, TColor> _signedDistanceToColor;

        public SignedDistanceGeometrySceneLayer(
            ISignedPointDistanceProvider source,
            Func<float, TColor> signedDistanceToColor,
            Func<TColor, TColor, TColor> blend)
            : base(blend)
        {
            _source = source;
            _signedDistanceToColor = signedDistanceToColor;
        }

        public override TColor Sample(PointXY point)
        {
            return _signedDistanceToColor(_source.SignedDistance(point));
        }
    }
}
