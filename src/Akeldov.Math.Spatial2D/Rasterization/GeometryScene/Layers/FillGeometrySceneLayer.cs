using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class FillGeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly ISignedPointDistanceProvider _source;
        private readonly TColor _color;
        private readonly float _edgeFalloff;
        private readonly Func<TColor, float, TColor> _applyCoverage;

        public FillGeometrySceneLayer(
            ISignedPointDistanceProvider source,
            TColor color,
            float edgeFalloff,
            Func<TColor, float, TColor> applyCoverage)
        {
            _source = source;
            _color = color;
            _edgeFalloff = edgeFalloff;
            _applyCoverage = applyCoverage;
        }

        public TColor Sample(PointXY point)
        {
            float coverage = GeometrySceneCoverage.GetFillCoverage(
                _source.SignedDistance(point),
                _edgeFalloff);

            return _applyCoverage(_color, coverage);
        }
    }
}
