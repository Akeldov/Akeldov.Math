using System;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal readonly struct ParameterizedCurveRasterSampler<TValue> : ISpatialRasterSampler<TValue>
    {
        private readonly IParameterizedCurve _source;
        private readonly Func<float, float, TValue> _projectionToValue;

        public ParameterizedCurveRasterSampler(IParameterizedCurve source, Func<float, float, TValue> projectionToValue)
        {
            _source = source;
            _projectionToValue = projectionToValue;
        }

        public TValue Sample(PointXY point)
        {
            ParameterizedCurveProjection projection = _source.ProjectWithParameter(point);
            return _projectionToValue(projection.Distance, projection.CurveCoordinate);
        }
    }
}
