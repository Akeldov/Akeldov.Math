using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TextGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly TextSignedDistanceProvider[] _texts;
        private readonly Func<float, TColor> _signedDistanceToColor;
        private readonly float _maxDistance;

        public TextGeometrySceneLayer(
            TextSignedDistanceProvider text,
            Func<float, TColor> signedDistanceToColor)
        {
            _texts = new[] { text ?? throw new ArgumentNullException(nameof(text)) };
            _signedDistanceToColor = signedDistanceToColor ?? throw new ArgumentNullException(nameof(signedDistanceToColor));
            _maxDistance = float.PositiveInfinity;
        }

        public TextGeometrySceneLayer(
            IReadOnlyList<TextSignedDistanceProvider> texts,
            Func<float, TColor> signedDistanceToColor)
        {
            if (texts == null)
                throw new ArgumentNullException(nameof(texts));
            if (texts.Count == 0)
                throw new ArgumentException("Text provider collection must not be empty.", nameof(texts));

            _texts = new TextSignedDistanceProvider[texts.Count];
            for (int i = 0; i < texts.Count; i++)
                _texts[i] = texts[i] ?? throw new ArgumentException("Text provider collection cannot contain null.", nameof(texts));

            _signedDistanceToColor = signedDistanceToColor ?? throw new ArgumentNullException(nameof(signedDistanceToColor));
            _maxDistance = float.PositiveInfinity;
        }

        public TextGeometrySceneLayer(
            IReadOnlyList<TextSignedDistanceProvider> texts,
            Func<float, TColor> signedDistanceToColor,
            float maxDistance)
            : this(texts, signedDistanceToColor)
        {
            if (maxDistance < 0f || float.IsNaN(maxDistance))
                throw new ArgumentOutOfRangeException(nameof(maxDistance));

            _maxDistance = maxDistance;
        }

        public TColor Sample(PointXY point)
        {
            int nearestIndex = 0;
            float nearestBoundsDistance = _texts[0].DistanceToBounds(point);

            for (int i = 1; i < _texts.Length; i++)
            {
                float boundsDistance = _texts[i].DistanceToBounds(point);
                if (boundsDistance < nearestBoundsDistance)
                {
                    nearestIndex = i;
                    nearestBoundsDistance = boundsDistance;
                }
            }

            if (nearestBoundsDistance > _maxDistance)
                return _signedDistanceToColor(float.PositiveInfinity);

            float minimum = _texts[nearestIndex].SignedDistance(point);

            for (int i = 0; i < _texts.Length; i++)
            {
                if (i == nearestIndex)
                    continue;

                TextSignedDistanceProvider text = _texts[i];
                float boundsDistance = text.DistanceToBounds(point);
                if ((minimum >= 0f && boundsDistance >= minimum) ||
                    (minimum < 0f && boundsDistance > 0f))
                {
                    continue;
                }

                float distance = text.SignedDistance(point);
                if (distance < minimum)
                    minimum = distance;
            }

            return _signedDistanceToColor(minimum);
        }
    }
}
