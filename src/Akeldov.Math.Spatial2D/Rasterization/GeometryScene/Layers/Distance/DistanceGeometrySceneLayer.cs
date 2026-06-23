using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class DistanceGeometrySceneLayer<TColor> : GeometrySceneLayer<TColor>
    {
        private readonly IPointDistanceProvider _source;
        private readonly Func<float, TColor> _distanceToColor;

        public DistanceGeometrySceneLayer(
            IPointDistanceProvider source,
            Func<float, TColor> distanceToColor,
            Func<TColor, TColor, TColor> blend)
            : base(blend)
        {
            _source = source;
            _distanceToColor = distanceToColor;
        }

        public override TColor Sample(PointXY point)
        {
            return _distanceToColor(_source.Distance(point));
        }
    }
}
