using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides reusable signed-distance geometry for laid-out TrueType text.
    /// </summary>
    public sealed class TextSignedDistanceProvider : ISignedPointDistanceProvider
    {
        private readonly ContourBasedRegion? _region;
        private readonly float _minX;
        private readonly float _minY;
        private readonly float _maxX;
        private readonly float _maxY;

        internal TextSignedDistanceProvider(
            IReadOnlyList<Akeldov.Math.Spatial2D.Contours.IContour> contours,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            if (contours == null)
                throw new ArgumentNullException(nameof(contours));

            _region = contours.Count == 0
                ? null
                : new ContourBasedRegion(contours);
            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
        }

        /// <summary>
        /// Creates laid-out TrueType text that can be reused by geometry scene layers.
        /// </summary>
        public static TextSignedDistanceProvider Create(
            TrueTypeFont font,
            string text,
            PointXY origin,
            float fontSize,
            TextLayoutOptions? layout = null)
        {
            return TrueTypeTextLayout.CreateText(
                font,
                text,
                origin,
                fontSize,
                layout ?? new TextLayoutOptions());
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            if (_region == null)
            {
                PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");
                return float.PositiveInfinity;
            }

            return _region.Distance(point);
        }

        /// <summary>Determines whether the point lies inside the text fill.</summary>
        public bool Contains(PointXY point) => _region != null && _region.Contains(point);

        /// <summary>Determines whether the point lies inside the text bounds expanded by padding.</summary>
        public bool IsWithinBounds(PointXY point, float padding) =>
            point.X >= _minX - padding &&
            point.X <= _maxX + padding &&
            point.Y >= _minY - padding &&
            point.Y <= _maxY + padding;

        internal float DistanceToBounds(PointXY point)
        {
            float dx = point.X < _minX ? _minX - point.X : point.X > _maxX ? point.X - _maxX : 0f;
            float dy = point.Y < _minY ? _minY - point.Y : point.Y > _maxY ? point.Y - _maxY : 0f;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            if (_region == null)
            {
                GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));
                PointXYValidation.ThrowIfNotFinite(
                    point,
                    nameof(point),
                    "Point coordinates must be finite.");

                return float.PositiveInfinity;
            }

            return _region.SignedDistance(point, geometryEpsilon);
        }
    }
}
