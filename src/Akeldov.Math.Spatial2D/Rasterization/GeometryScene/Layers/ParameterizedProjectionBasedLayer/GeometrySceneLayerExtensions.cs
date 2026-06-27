using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class GeometrySceneLayerExtensions
    {
        /// <summary>
        /// Adds a parameterized projection layer.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <typeparam name="TSource">The parameterized curve type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="source">The parameterized curve to sample.</param>
        /// <param name="projectionToColor">
        /// The function that maps the sampled source point and nearest projection result to a color.
        /// Projection distance and curve coordinate are expressed in world coordinate units.
        /// </param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> AddParameterizedProjectionBasedLayer<TColor, TSource>(
            this GeometryScene<TColor> scene,
            TSource source,
            Func<PointXY, ParameterizedCurveProjection, TColor> projectionToColor)
            where TSource : IParameterizedCurve
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (projectionToColor == null)
                throw new ArgumentNullException(nameof(projectionToColor));

            return scene.AddLayer(new ParameterizedProjectionBasedLayer<TColor, TSource>(
                new List<TSource> { source },
                projectionToColor));
        }

        /// <summary>
        /// Adds a nearest parameterized projection layer for a structural source list.
        /// </summary>
        /// <typeparam name="TColor">The scene color value type.</typeparam>
        /// <typeparam name="TSource">The parameterized curve type.</typeparam>
        /// <param name="scene">The scene to add the layer to.</param>
        /// <param name="sources">The parameterized curves to sample. The list is copied into retained layer state.</param>
        /// <param name="projectionToColor">
        /// The function that maps the sampled source point and nearest projection result to a color.
        /// Projection distance and curve coordinate are expressed in world coordinate units.
        /// </param>
        /// <returns><paramref name="scene"/>.</returns>
        public static GeometryScene<TColor> AddParameterizedProjectionBasedLayer<TColor, TSource>(
            this GeometryScene<TColor> scene,
            IReadOnlyList<TSource> sources,
            Func<PointXY, ParameterizedCurveProjection, TColor> projectionToColor)
            where TSource : IParameterizedCurve
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (projectionToColor == null)
                throw new ArgumentNullException(nameof(projectionToColor));

            return scene.AddLayer(new ParameterizedProjectionBasedLayer<TColor, TSource>(
                sources,
                projectionToColor));
        }
    }
}
