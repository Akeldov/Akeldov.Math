using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TextSignedDistanceProvider : ISignedPointDistanceProvider
    {
        private readonly ContourBasedRegion? _region;
        private readonly float _minX;
        private readonly float _minY;
        private readonly float _maxX;
        private readonly float _maxY;

        public TextSignedDistanceProvider(
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

        public float Distance(PointXY point)
        {
            if (_region == null)
            {
                PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");
                return float.PositiveInfinity;
            }

            return _region.Distance(point);
        }

        public bool Contains(PointXY point) => _region != null && _region.Contains(point);

        public bool IsWithinBounds(PointXY point, float padding) =>
            point.X >= _minX - padding &&
            point.X <= _maxX + padding &&
            point.Y >= _minY - padding &&
            point.Y <= _maxY + padding;

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
