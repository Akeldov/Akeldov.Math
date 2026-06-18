using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a closed two-dimensional composite contour with a length-based curve coordinate.
    /// </summary>
    public sealed class ParameterizedCompositeContour : IParameterizedCompositeContour
    {
        private readonly CompositeContour _contour;
        private readonly IReadOnlyList<IFinitePath> _curves;
        private readonly float[] _curveStartCoordinates;

        /// <summary>
        /// Initializes a new parameterized composite contour from the specified finite paths.
        /// </summary>
        /// <param name="curves">The finite paths that form the contour.</param>
        public ParameterizedCompositeContour(IReadOnlyList<IFinitePath> curves)
        {
            _contour = new CompositeContour(curves);
            _curves = _contour.Curves;
            _curveStartCoordinates = new float[_curves.Count];

            float curveStartCoordinate = 0f;
            for (int i = 0; i < _curves.Count; i++)
            {
                _curveStartCoordinates[i] = curveStartCoordinate;

                curveStartCoordinate += _curves[i].Length;
            }
        }

        /// <summary>
        /// Gets the read-only structural view of the finite paths that form this contour.
        /// </summary>
        public IReadOnlyList<IFinitePath> Curves => _curves;

        /// <summary>
        /// Gets the finite non-negative contour boundary length in world coordinate units.
        /// </summary>
        public float Length => _contour.Length;

        /// <summary>
        /// Gets the point at the start of the contour traversal.
        /// </summary>
        public PointXY StartPoint => _curves[0].StartPoint;

        /// <summary>
        /// Gets the point at the end of the contour traversal.
        /// </summary>
        public PointXY EndPoint => _curves[_curves.Count - 1].EndPoint;

        /// <summary>
        /// Gets one endpoint of this closed path.
        /// </summary>
        public PointXY EndpointA => StartPoint;

        /// <summary>
        /// Gets the other endpoint of this closed path.
        /// </summary>
        public PointXY EndpointB => EndPoint;

        /// <inheritdoc/>
        public bool Encloses(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.Encloses(point, geometryEpsilon);
        }

        /// <inheritdoc/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.GetRayIntersections(ray, geometryEpsilon);
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            return _contour.Project(point);
        }

        /// <inheritdoc/>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            ParameterizedCurveProjection closestProjection = ProjectOnCurve(0, point);

            for (int i = 1; i < _curves.Count; i++)
            {
                ParameterizedCurveProjection projection = ProjectOnCurve(i, point);
                if (projection.Distance < closestProjection.Distance)
                    closestProjection = projection;
            }

            return closestProjection;
        }

        /// <inheritdoc/>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the contour length.");

            float remainingCoordinate = curveCoordinate;

            for (int i = 0; i < _curves.Count; i++)
            {
                IFinitePath curve = _curves[i];
                float curveLength = curve.Length;

                if (remainingCoordinate <= curveLength || i == _curves.Count - 1)
                    return curve.GetPoint(remainingCoordinate);

                remainingCoordinate -= curveLength;
            }

            return _curves[0].GetPoint(0f);
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            return _contour.Distance(point);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.SignedDistance(point, geometryEpsilon);
        }

        private ParameterizedCurveProjection ProjectOnCurve(int curveIndex, PointXY point)
        {
            ParameterizedCurveProjection projection = _curves[curveIndex].ProjectWithParameter(point);
            float contourCoordinate = _curveStartCoordinates[curveIndex] + projection.CurveCoordinate;

            return new ParameterizedCurveProjection(
                projection.ProjectedPoint,
                contourCoordinate,
                projection.Distance);
        }
    }
}
