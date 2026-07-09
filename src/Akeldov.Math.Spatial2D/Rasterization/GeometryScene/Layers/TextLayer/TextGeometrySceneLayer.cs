using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TextGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly TextSignedDistanceProvider _text;
        private readonly Func<float, TColor> _signedDistanceToColor;

        public TextGeometrySceneLayer(
            TextSignedDistanceProvider text,
            Func<float, TColor> signedDistanceToColor)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _signedDistanceToColor = signedDistanceToColor ?? throw new ArgumentNullException(nameof(signedDistanceToColor));
        }

        public TColor Sample(PointXY point)
        {
            return _signedDistanceToColor(_text.SignedDistance(point));
        }
    }
}
