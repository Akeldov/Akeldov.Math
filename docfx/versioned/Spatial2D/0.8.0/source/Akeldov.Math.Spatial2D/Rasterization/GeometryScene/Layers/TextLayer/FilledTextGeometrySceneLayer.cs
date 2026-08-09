using Akeldov.Math.Spatial2D.Imaging;
using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class FilledTextGeometrySceneLayer : IGeometrySceneLayer<RGBA16BitColor>
    {
        private readonly TextSignedDistanceProvider _text;
        private readonly RGBA16BitColor _color;
        private readonly RGBA16BitColor _transparentColor;
        private readonly float _edgeFalloff;

        public FilledTextGeometrySceneLayer(
            TextSignedDistanceProvider text,
            RGBA16BitColor color,
            float edgeFalloff)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _color = color;
            _transparentColor = color.ScaleAlpha(0f);
            _edgeFalloff = edgeFalloff;
        }

        public RGBA16BitColor Sample(PointXY point)
        {
            if (!_text.IsWithinBounds(point, _edgeFalloff))
                return _transparentColor;

            if (_text.Contains(point))
                return _color;

            float distance = _text.Distance(point);
            if (distance >= _edgeFalloff || float.IsPositiveInfinity(distance))
                return _transparentColor;

            return _color.ScaleAlpha(1f - distance / _edgeFalloff);
        }
    }
}
