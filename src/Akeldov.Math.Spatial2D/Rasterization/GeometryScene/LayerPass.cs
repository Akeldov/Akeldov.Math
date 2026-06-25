using System;
namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Pairs a geometry scene layer with the function used to blend that layer into a scene.
    /// </summary>
    /// <typeparam name="TColor">The color value type produced by the layer.</typeparam>
    public sealed class LayerPass<TColor>
    {
        /// <summary>
        /// Gets the geometry scene layer.
        /// </summary>
        public IGeometrySceneLayer<TColor> Layer { get; }

        /// <summary>
        /// Gets the function that composites the current scene color with this layer's sampled color.
        /// </summary>
        public Func<TColor, TColor, TColor> Blend { get; }

        /// <summary>
        /// Initializes a new layer pass.
        /// </summary>
        /// <param name="layer">The geometry scene layer.</param>
        /// <param name="blend">The blend function for this layer.</param>
        public LayerPass(
            IGeometrySceneLayer<TColor> layer,
            Func<TColor, TColor, TColor> blend)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            Blend = blend ?? throw new ArgumentNullException(nameof(blend));
        }
    }
}
