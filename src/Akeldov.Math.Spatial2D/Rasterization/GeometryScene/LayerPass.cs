using System;
using System.Collections.Generic;
using System.Text;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public sealed class LayerPass<TColor>
    {
        public IGeometrySceneLayer<TColor> Layer { get; }

        public Func<TColor, TColor, TColor> Blend { get; }

        public LayerPass(
            IGeometrySceneLayer<TColor> layer,
            Func<TColor, TColor, TColor> blend)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            Blend = blend ?? throw new ArgumentNullException(nameof(blend));
        }
    }
}
