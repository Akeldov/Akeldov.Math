using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class PointMarkerCollectionGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly PointXY[] _points;
        private readonly TColor _color;
        private readonly float _radius;
        private readonly float _edgeFalloff;
        private readonly Func<TColor, float, TColor> _applyCoverage;

        public PointMarkerCollectionGeometrySceneLayer(
            PointXY[] points,
            TColor color,
            float radius,
            float edgeFalloff,
            Func<TColor, float, TColor> applyCoverage)
        {
            _points = points;
            _color = color;
            _radius = radius;
            _edgeFalloff = edgeFalloff;
            _applyCoverage = applyCoverage;
        }

        public TColor Sample(PointXY point)
        {
            float distance = _points[0].Distance(point);

            for (int i = 1; i < _points.Length; i++)
            {
                float candidateDistance = _points[i].Distance(point);
                if (candidateDistance < distance)
                    distance = candidateDistance;
            }

            float coverage = GeometrySceneCoverage.GetOutsideCoverage(
                distance,
                _radius,
                _edgeFalloff);

            return _applyCoverage(_color, coverage);
        }
    }
}
