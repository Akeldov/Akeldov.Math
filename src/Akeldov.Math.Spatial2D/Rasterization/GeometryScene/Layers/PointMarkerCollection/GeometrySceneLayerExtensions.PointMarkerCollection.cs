using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a hard-edged point marker layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="point">The finite point marker center.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Point<TColor>(
            this GeometryScene<TColor> scene,
            PointXY point,
            TColor color,
            float radius)
        {
            return Point(scene, point, color, radius, 0f);
        }

        /// <summary>
        /// Adds a point marker layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="point">The finite point marker center.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the marker, in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Point<TColor>(
            this GeometryScene<TColor> scene,
            PointXY point,
            TColor color,
            float radius,
            float edgeFalloff)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point marker coordinates must be finite.");

            GeometrySceneValidation.ValidatePositiveFinite(radius, nameof(radius), "Point marker radius must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Point marker edge falloff must be finite and non-negative.");

            return scene.AddLayer(new PointMarkerCollectionGeometrySceneLayer<TColor>(
                new[] { point },
                color,
                radius,
                edgeFalloff,
                scene.ApplyCoverage,
                scene.DefaultLayerBlend));
        }

        /// <summary>
        /// Adds a hard-edged point marker layer for a copied set of finite points.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="points">The finite point marker centers. The list is validated and copied when the layer is added.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Point<TColor>(
            this GeometryScene<TColor> scene,
            IReadOnlyList<PointXY> points,
            TColor color,
            float radius)
        {
            return Point(scene, points, color, radius, 0f);
        }

        /// <summary>
        /// Adds a point marker layer for a copied set of finite points.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="points">The finite point marker centers. The list is validated and copied when the layer is added.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside each marker, in world coordinate units.</param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> Point<TColor>(
            this GeometryScene<TColor> scene,
            IReadOnlyList<PointXY> points,
            TColor color,
            float radius,
            float edgeFalloff)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            PointXY[] pointCopy = CopyPointMarkers(points, nameof(points));

            GeometrySceneValidation.ValidatePositiveFinite(radius, nameof(radius), "Point marker radius must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Point marker edge falloff must be finite and non-negative.");

            return scene.AddLayer(new PointMarkerCollectionGeometrySceneLayer<TColor>(
                pointCopy,
                color,
                radius,
                edgeFalloff,
                scene.ApplyCoverage,
                scene.DefaultLayerBlend));
        }

        private static PointXY[] CopyPointMarkers(IReadOnlyList<PointXY> points, string parameterName)
        {
            if (points == null)
                throw new ArgumentNullException(parameterName);

            if (points.Count == 0)
                throw new ArgumentException("Point marker collection must not be empty.", parameterName);

            var copy = new PointXY[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                PointXY point = points[i];
                if (!PointXYValidation.IsFinite(point))
                    throw new ArgumentException("Point marker coordinates must be finite.", parameterName);

                copy[i] = point;
            }

            return copy;
        }
    }
}
