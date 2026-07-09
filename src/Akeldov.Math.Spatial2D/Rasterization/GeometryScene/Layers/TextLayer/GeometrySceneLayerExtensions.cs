using Akeldov.Math.Spatial2D.Imaging;
using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a TrueType text layer whose color is mapped from signed distance to the text outline.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="font">The TrueType font to render.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="origin">The text origin in world coordinates.</param>
        /// <param name="fontSize">The font em size, in world coordinate units.</param>
        /// <param name="signedDistanceToColor">The function that maps signed distance in world coordinate units to a color.</param>
        /// <param name="layout">The optional text layout options.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> AddTextLayer<TColor>(
            this GeometryScene<TColor> scene,
            TrueTypeFont font,
            string text,
            PointXY origin,
            float fontSize,
            Func<float, TColor> signedDistanceToColor,
            TextLayoutOptions? layout = null)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (signedDistanceToColor == null)
                throw new ArgumentNullException(nameof(signedDistanceToColor));

            TextSignedDistanceProvider textProvider = TrueTypeTextLayout.CreateText(
                font,
                text,
                origin,
                fontSize,
                layout ?? new TextLayoutOptions());

            return scene.AddLayer(new TextGeometrySceneLayer<TColor>(textProvider, signedDistanceToColor));
        }

        /// <summary>
        /// Adds a filled TrueType text layer with an alpha falloff outside the text outline.
        /// </summary>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="font">The TrueType font to render.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="origin">The text origin in world coordinates.</param>
        /// <param name="fontSize">The font em size, in world coordinate units.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="edgeFalloff">The finite positive alpha falloff outside the text outline, in world coordinate units.</param>
        /// <param name="anchor">The text anchor used to position laid-out text relative to <paramref name="origin"/>.</param>
        /// <param name="letterSpacing">The additional spacing between adjacent glyph advances, in world coordinate units.</param>
        /// <param name="lineSpacing">The additional spacing between line advances, in world coordinate units.</param>
        /// <param name="useKerning">Whether legacy TrueType kerning pairs should be applied when available.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddTextLayer(
            this GeometryScene<RGBA16BitColor> scene,
            TrueTypeFont font,
            string text,
            PointXY origin,
            float fontSize,
            RGBA16BitColor color,
            float edgeFalloff,
            TextAnchor anchor = TextAnchor.BaselineLeft,
            float letterSpacing = 0f,
            float lineSpacing = 0f,
            bool useKerning = true)
        {
            return scene.AddTextLayer(
                font,
                text,
                origin,
                fontSize,
                color,
                edgeFalloff,
                new TextLayoutOptions
                {
                    Anchor = anchor,
                    LetterSpacing = letterSpacing,
                    LineSpacing = lineSpacing,
                    UseKerning = useKerning
                });
        }

        /// <summary>
        /// Adds a filled TrueType text layer with an alpha falloff outside the text outline.
        /// </summary>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="font">The TrueType font to render.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="origin">The text origin in world coordinates.</param>
        /// <param name="fontSize">The font em size, in world coordinate units.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="edgeFalloff">The finite positive alpha falloff outside the text outline, in world coordinate units.</param>
        /// <param name="layout">The text layout options.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<RGBA16BitColor> AddTextLayer(
            this GeometryScene<RGBA16BitColor> scene,
            TrueTypeFont font,
            string text,
            PointXY origin,
            float fontSize,
            RGBA16BitColor color,
            float edgeFalloff,
            TextLayoutOptions layout)
        {
            if (edgeFalloff <= 0f || float.IsNaN(edgeFalloff) || float.IsInfinity(edgeFalloff))
                throw new ArgumentOutOfRangeException(nameof(edgeFalloff), "Text layer edge falloff must be finite and positive.");

            return scene.AddTextLayer(
                font,
                text,
                origin,
                fontSize,
                d => d <= 0f ? color : ApplyOutsideFalloff(color, d, edgeFalloff),
                layout);
        }

        private static RGBA16BitColor ApplyOutsideFalloff(
            RGBA16BitColor color,
            float distance,
            float edgeFalloff)
        {
            if (float.IsPositiveInfinity(distance))
                return color.ScaleAlpha(0f);

            float outsideDistance = MathF.Max(0f, distance);
            return color.ScaleAlpha(1f - MathF.Min(outsideDistance, edgeFalloff) / edgeFalloff);
        }
    }
}
