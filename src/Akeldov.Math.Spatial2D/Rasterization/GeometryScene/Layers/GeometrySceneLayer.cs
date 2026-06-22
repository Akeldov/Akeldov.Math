using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal abstract class GeometrySceneLayer<TColor> : IGeometrySceneLayer<TColor>
    {
        private readonly Func<TColor, TColor, TColor> _blend;

        protected GeometrySceneLayer(Func<TColor, TColor, TColor> blend)
        {
            if (blend == null)
                throw new ArgumentNullException(nameof(blend));

            _blend = blend;
        }

        public TColor Blend(TColor background, TColor foreground)
        {
            return _blend(background, foreground);
        }

        public abstract TColor Sample(PointXY point);
    }
}
