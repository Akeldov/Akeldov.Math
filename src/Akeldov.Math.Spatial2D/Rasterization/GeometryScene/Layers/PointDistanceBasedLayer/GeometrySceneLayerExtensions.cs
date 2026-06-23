using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
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
        public static GeometryScene<TColor> AddPointDistanceBasedLayer<TColor, TSource>(
            this GeometryScene<TColor> scene,
            TSource source,
            Func<float, TColor> distanceToColor)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (distanceToColor == null)
                throw new ArgumentNullException(nameof(distanceToColor));

            return scene.AddLayer(new PointDistanceBasedLayer<TColor, TSource>(
                new List<TSource>() { source },
                distanceToColor));
        }

        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color,
            float width)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource>() { source },
                d => d <= width ? color : color.ScaleAlpha(0f)));
        }

        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color,
            float width,
            float edgeFalloff)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource>() { source },
                d => d <= width ? color : color.ScaleAlpha(1f - MathF.Min(d, edgeFalloff) / edgeFalloff)));
        }

        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            IReadOnlyList<TSource> sources,
            RGBA16BitColor color,
            float width,
            float edgeFalloff)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                sources,
                d => d <= width 
                    ? color 
                    : color.ScaleAlpha(1f - MathF.Min(d, edgeFalloff) / edgeFalloff)));
        }
    }
}
