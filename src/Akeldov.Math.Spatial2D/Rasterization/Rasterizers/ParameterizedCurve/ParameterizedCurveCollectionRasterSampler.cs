using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct ParameterizedCurveCollectionRasterSampler<TValue> : ISpatialRasterSampler<TValue>
    {
        private readonly IReadOnlyList<IParameterizedCurve> _curves;
        private readonly Func<float, float, TValue> _projectionToValue;

        public ParameterizedCurveCollectionRasterSampler(IReadOnlyList<IParameterizedCurve> curves, Func<float, float, TValue> projectionToValue)
        {
            _curves = curves;
            _projectionToValue = projectionToValue;
        }

        public TValue Sample(PointXY point)
        {
            ParameterizedCurveProjection nearestProjection = _curves[0].ProjectWithParameter(point);

            for (int i = 1; i < _curves.Count; i++)
            {
                ParameterizedCurveProjection projection = _curves[i].ProjectWithParameter(point);
                if (projection.Distance < nearestProjection.Distance)
                    nearestProjection = projection;
            }

            return _projectionToValue(nearestProjection.Distance, nearestProjection.CurveCoordinate);
        }
    }
}
