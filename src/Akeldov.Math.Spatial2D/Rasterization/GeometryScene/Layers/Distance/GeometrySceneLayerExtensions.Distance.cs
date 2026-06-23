using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides convenience methods for adding standard geometry scene layers.
    /// </summary>
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds an unsigned point-distance layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The distance provider to sample.</param>
        /// <param name="distanceToColor">The function that maps unsigned distance in world coordinate units to a color.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Distance<TColor>(
            this GeometryScene<TColor> scene,
            IPointDistanceProvider source,
            Func<float, TColor> distanceToColor)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (distanceToColor == null)
                throw new ArgumentNullException(nameof(distanceToColor));

            return scene.AddLayer(new DistanceGeometrySceneLayer<TColor>(
                source,
                distanceToColor,
                scene.DefaultLayerBlend));
        }
    }
}
