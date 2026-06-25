using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides helpers for adding distance-based layers to geometry scenes.
    /// </summary>
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds an unsigned point-distance layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <typeparam name="TSource">The distance provider type.</typeparam>
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

        /// <summary>
        /// Adds a hard-edged unsigned point-distance layer.
        /// </summary>
        /// <typeparam name="TSource">The distance provider type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The distance provider to sample.</param>
        /// <param name="color">The color used within <paramref name="fillDistance"/> world coordinate units of the source.</param>
        /// <param name="fillDistance">The finite non-negative fill distance threshold in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color,
            float fillDistance)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ThrowIfNotFiniteNonNegative(fillDistance, nameof(fillDistance), "Distance layer fill distance");

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource>() { source },
                d => d <= fillDistance ? color : color.ScaleAlpha(0f)));
        }

        /// <summary>
        /// Adds an unsigned point-distance layer with an alpha falloff outside the distance threshold.
        /// </summary>
        /// <typeparam name="TSource">The distance provider type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The distance provider to sample.</param>
        /// <param name="color">The color used within <paramref name="fillDistance"/> world coordinate units of the source.</param>
        /// <param name="fillDistance">The finite non-negative fill distance threshold in world coordinate units.</param>
        /// <param name="edgeFalloff">The finite positive alpha falloff distance in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color,
            float fillDistance,
            float edgeFalloff)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ThrowIfNotFiniteNonNegative(fillDistance, nameof(fillDistance), "Distance layer fill distance");
            ThrowIfNotFinitePositive(edgeFalloff, nameof(edgeFalloff), "Distance layer edge falloff");

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource>() { source },
                d => ApplyOutsideFalloff(color, d, fillDistance, edgeFalloff)));
        }

        /// <summary>
        /// Adds an unsigned point-distance layer for a structural source list with an alpha falloff outside the distance threshold.
        /// </summary>
        /// <typeparam name="TSource">The distance provider type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="sources">The distance providers to sample. The list is copied into retained layer state.</param>
        /// <param name="color">The color used within <paramref name="fillDistance"/> world coordinate units of the nearest source.</param>
        /// <param name="fillDistance">The finite non-negative fill distance threshold in world coordinate units.</param>
        /// <param name="edgeFalloff">The finite positive alpha falloff distance in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            IReadOnlyList<TSource> sources,
            RGBA16BitColor color,
            float fillDistance,
            float edgeFalloff)
            where TSource : IPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            ThrowIfNotFiniteNonNegative(fillDistance, nameof(fillDistance), "Distance layer fill distance");
            ThrowIfNotFinitePositive(edgeFalloff, nameof(edgeFalloff), "Distance layer edge falloff");

            return scene.AddLayer(new PointDistanceBasedLayer<RGBA16BitColor, TSource>(
                sources,
                d => d <= fillDistance
                    ? color 
                    : ApplyOutsideFalloff(color, d, fillDistance, edgeFalloff)));
        }

        private static RGBA16BitColor ApplyOutsideFalloff(
            RGBA16BitColor color,
            float distance,
            float fullCoverageDistance,
            float edgeFalloff)
        {
            if (distance <= fullCoverageDistance)
                return color;

            float outsideDistance = distance - fullCoverageDistance;
            return color.ScaleAlpha(1f - MathF.Min(outsideDistance, edgeFalloff) / edgeFalloff);
        }

        private static void ThrowIfNotFiniteNonNegative(float value, string paramName, string description)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(paramName, $"{description} must be finite and non-negative.");
        }

        private static void ThrowIfNotFinitePositive(float value, string paramName, string description)
        {
            if (value <= 0f || float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(paramName, $"{description} must be finite and positive.");
        }
    }
}
