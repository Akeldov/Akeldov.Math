using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class PointMarkerGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly PointXY _point;
        private readonly TColor _color;
        private readonly float _radius;
        private readonly float _edgeFalloff;
        private readonly Func<TColor, float, TColor> _applyCoverage;

        public PointMarkerGeometrySceneLayer(
            PointXY point,
            TColor color,
            float radius,
            float edgeFalloff,
            Func<TColor, float, TColor> applyCoverage)
        {
            _point = point;
            _color = color;
            _radius = radius;
            _edgeFalloff = edgeFalloff;
            _applyCoverage = applyCoverage;
        }

        public TColor Sample(PointXY point)
        {
            float coverage = GeometrySceneCoverage.GetOutsideCoverage(
                _point.Distance(point),
                _radius,
                _edgeFalloff);

            return _applyCoverage(_color, coverage);
        }
    }
}
