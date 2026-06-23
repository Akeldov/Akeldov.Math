using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a hard-edged filled signed-distance provider layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Fill<TColor>(
            this GeometryScene<TColor> scene,
            ISignedPointDistanceProvider source,
            TColor color)
        {
            return Fill(scene, source, color, 0f);
        }

        /// <summary>
        /// Adds a filled signed-distance provider layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the filled boundary, in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Fill<TColor>(
            this GeometryScene<TColor> scene,
            ISignedPointDistanceProvider source,
            TColor color,
            float edgeFalloff)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Fill edge falloff must be finite and non-negative.");

            return scene.AddLayer(new FillGeometrySceneLayer<TColor>(
                source,
                color,
                edgeFalloff,
                scene.ApplyCoverage,
                scene.DefaultLayerBlend));
        }
    }
}
