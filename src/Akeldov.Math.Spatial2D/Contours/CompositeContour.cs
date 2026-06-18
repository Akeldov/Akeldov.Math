using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional contour made from finite paths.
    /// </summary>
    public sealed class CompositeContour : ICompositeContour
    {
        private readonly IFinitePath[] _curves;
        private readonly IReadOnlyList<IFinitePath> _readOnlyCurves;
        private readonly float _length;

        /// <summary>
        /// Initializes a new composite contour from the specified finite paths.
        /// </summary>
        /// <param name="curves">The finite paths that form the contour.</param>
        public CompositeContour(IReadOnlyList<IFinitePath> curves)
        {
            if (curves == null)
                throw new ArgumentNullException(nameof(curves));

            if (curves.Count == 0)
                throw new ArgumentException("A contour must contain at least one curve.", nameof(curves));

            _curves = new IFinitePath[curves.Count];

            for (int i = 0; i < curves.Count; i++)
            {
                _curves[i] = curves[i] ?? throw new ArgumentException("A contour cannot contain null curves.", nameof(curves));
            }

            ValidateCurvesFormClosedChain(_curves, nameof(curves));
            _length = GetLength(_curves, nameof(curves));

            _readOnlyCurves = Array.AsReadOnly(_curves);
        }

        /// <summary>
        /// Gets the read-only structural view of the finite paths that form this contour.
        /// </summary>
        public IReadOnlyList<IFinitePath> Curves => _readOnlyCurves;

        /// <summary>
        /// Gets the finite non-negative contour boundary length in world coordinate units.
        /// </summary>
        public float Length => _length;

        /// <inheritdoc/>
        public bool Encloses(PointXY point, float geometryEpsilon = 1E-06F)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            var ray = new Ray(point);
            var intersections = new List<PointXY>();

            for (int i = 0; i < _curves.Length; i++)
            {
                IFinitePath curve = _curves[i];

                if (curve.Distance(point) <= geometryEpsilon)
                    return true;

                List<PointXY> curveIntersections = curve.GetRayIntersections(ray, geometryEpsilon);
                if (curveIntersections == null)
                    continue;

                for (int j = 0; j < curveIntersections.Count; j++)
                {
                    PointXY intersection = curveIntersections[j];
                    if (intersection.X <= point.X + geometryEpsilon)
                        continue;

                    AddDistinct(intersections, intersection, geometryEpsilon);
                }
            }

            return intersections.Count % 2 == 1;
        }

        /// <inheritdoc/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();

            for (int i = 0; i < _curves.Length; i++)
            {
                List<PointXY> curveIntersections = _curves[i].GetRayIntersections(ray, geometryEpsilon);
                if (curveIntersections == null)
                    continue;

                for (int j = 0; j < curveIntersections.Count; j++)
                {
                    AddDistinct(intersections, curveIntersections[j], geometryEpsilon);
                }
            }

            return intersections;
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            CurveProjection closestProjection = _curves[0].Project(point);

            for (int i = 1; i < _curves.Length; i++)
            {
                CurveProjection projection = _curves[i].Project(point);
                if (projection.Distance < closestProjection.Distance)
                    closestProjection = projection;
            }

            return closestProjection;
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            float minDistance = float.MaxValue;

            for (int i = 0; i < _curves.Length; i++)
            {
                float distance = _curves[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = 1E-06F)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            float distance = Distance(point);
            return Encloses(point, geometryEpsilon) ? -distance : distance;
        }

        private static void ValidateCurvesFormClosedChain(IReadOnlyList<IFinitePath> curves, string parameterName)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                IFinitePath currentCurve = curves[i];
                IFinitePath nextCurve = curves[(i + 1) % curves.Count];

                if (!currentCurve.EndPoint.AlmostEquals(nextCurve.StartPoint))
                    throw new ArgumentException("CompositeContour curves must form a closed continuous chain.", parameterName);
            }
        }

        private static float GetLength(IReadOnlyList<IFinitePath> curves, string parameterName)
        {
            float length = 0f;

            for (int i = 0; i < curves.Count; i++)
            {
                float curveLength = curves[i].Length;
                if (curveLength < 0f || float.IsNaN(curveLength) || float.IsInfinity(curveLength))
                    throw new ArgumentException("CompositeContour curves must expose finite non-negative lengths.", parameterName);

                length += curveLength;
                if (float.IsInfinity(length))
                    throw new ArgumentException("CompositeContour length must be finite.", parameterName);
            }

            return length;
        }

        private static void AddDistinct(List<PointXY> intersections, PointXY point, float geometryEpsilon)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].AlmostEquals(point, geometryEpsilon))
                    return;
            }

            intersections.Add(point);
        }
    }
}
