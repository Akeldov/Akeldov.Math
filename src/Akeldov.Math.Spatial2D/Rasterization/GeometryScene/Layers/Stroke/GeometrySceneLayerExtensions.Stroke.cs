using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a hard-edged stroke around an unsigned point-distance provider.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The distance provider to stroke.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="width">The positive stroke width in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Stroke<TColor>(
            this GeometryScene<TColor> scene,
            IPointDistanceProvider source,
            TColor color,
            float width)
        {
            return Stroke(scene, source, color, width, 0f);
        }

        /// <summary>
        /// Adds a stroke around an unsigned point-distance provider.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The distance provider to stroke.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="width">The positive stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke, in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Stroke<TColor>(
            this GeometryScene<TColor> scene,
            IPointDistanceProvider source,
            TColor color,
            float width,
            float edgeFalloff)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            GeometrySceneValidation.ValidatePositiveFinite(width, nameof(width), "Stroke width must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Stroke edge falloff must be finite and non-negative.");

            return scene.AddLayer(new StrokeGeometrySceneLayer<TColor>(
                source,
                color,
                width,
                edgeFalloff,
                scene.ApplyCoverage,
                scene.DefaultLayerBlend));
        }
    }
}
