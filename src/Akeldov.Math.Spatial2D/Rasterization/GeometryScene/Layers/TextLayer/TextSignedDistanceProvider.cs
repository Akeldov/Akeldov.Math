using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class TextSignedDistanceProvider : ISignedPointDistanceProvider
    {
        private readonly ContourBasedRegion? _region;

        public TextSignedDistanceProvider(IReadOnlyList<Akeldov.Math.Spatial2D.Contours.IContour> contours)
        {
            if (contours == null)
                throw new ArgumentNullException(nameof(contours));

            _region = contours.Count == 0
                ? null
                : new ContourBasedRegion(contours);
        }

        public float Distance(PointXY point)
        {
            float signedDistance = SignedDistance(point);
            return float.IsPositiveInfinity(signedDistance) ? signedDistance : MathF.Abs(signedDistance);
        }

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
