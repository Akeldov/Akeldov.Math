using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;

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
        public static GeometryScene<TColor> AddSignedPointDistanceBasedLayer<TColor, TSource>(
            this GeometryScene<TColor> scene,
            TSource source,
            Func<float, TColor> signedDistanceToColor)
            where TSource : ISignedPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (signedDistanceToColor == null)
                throw new ArgumentNullException(nameof(signedDistanceToColor));

            return scene.AddLayer(new SignedPointDistanceBasedLayer<TColor, TSource>(
                new List<TSource> { source },
                signedDistanceToColor));
        }

        /// <summary>
        /// Adds a hard-edged filled signed-distance provider layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddSignedPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color)
            where TSource : ISignedPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return scene.AddLayer(new SignedPointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource> { source },
                d => d <= 0
                    ? color
                    : color.ScaleAlpha(0)));
        }

        /// <summary>
        /// Adds a filled signed-distance provider layer.
        /// </summary>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the filled boundary, in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddSignedPointDistanceBasedLayer<TSource>(
            this GeometryScene<RGBA16BitColor> scene,
            TSource source,
            RGBA16BitColor color,
            float edgeFalloff)
            where TSource : ISignedPointDistanceProvider
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (edgeFalloff < 0f || float.IsNaN(edgeFalloff) || float.IsInfinity(edgeFalloff))
                throw new ArgumentOutOfRangeException(nameof(edgeFalloff), "Fill edge falloff must be finite and non-negative.");

            return scene.AddLayer(new SignedPointDistanceBasedLayer<RGBA16BitColor, TSource>(
                new List<TSource> { source },
                d => d <= 0 
                    ? color 
                    : color.ScaleAlpha(1f - MathF.Min(d, edgeFalloff) / edgeFalloff)));
        }
    }
}
