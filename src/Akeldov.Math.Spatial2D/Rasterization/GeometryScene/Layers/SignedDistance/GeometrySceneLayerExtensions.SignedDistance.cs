using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a signed point-distance layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The signed distance provider to sample.</param>
        /// <param name="signedDistanceToColor">The function that maps signed distance in world coordinate units to a color.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> SignedDistance<TColor>(
            this GeometryScene<TColor> scene,
            ISignedPointDistanceProvider source,
            Func<float, TColor> signedDistanceToColor)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (signedDistanceToColor == null)
                throw new ArgumentNullException(nameof(signedDistanceToColor));

            return scene.AddLayer(new SignedDistanceGeometrySceneLayer<TColor>(
                source,
                signedDistanceToColor,
                scene.DefaultLayerBlend));
        }
    }
}
