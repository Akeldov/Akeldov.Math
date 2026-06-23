using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class StrokeGeometrySceneLayer<TColor> : GeometrySceneLayer<TColor>
    {
        private readonly IPointDistanceProvider _source;
        private readonly TColor _color;
        private readonly float _radius;
        private readonly float _edgeFalloff;
        private readonly Func<TColor, float, TColor> _applyCoverage;

        public StrokeGeometrySceneLayer(
            IPointDistanceProvider source,
            TColor color,
            float width,
            float edgeFalloff,
            Func<TColor, float, TColor> applyCoverage,
            Func<TColor, TColor, TColor> blend)
            : base(blend)
        {
            _source = source;
            _color = color;
            _radius = width * 0.5f;
            _edgeFalloff = edgeFalloff;
            _applyCoverage = applyCoverage;
        }

        public override TColor Sample(PointXY point)
        {
            float coverage = GeometrySceneCoverage.GetOutsideCoverage(
                _source.Distance(point),
                _radius,
                _edgeFalloff);

            return _applyCoverage(_color, coverage);
        }
    }
}
