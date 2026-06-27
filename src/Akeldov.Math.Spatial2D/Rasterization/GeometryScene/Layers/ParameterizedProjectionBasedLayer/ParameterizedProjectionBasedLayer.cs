using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal sealed class ParameterizedProjectionBasedLayer<TColor, TSource> : IGeometrySceneLayer<TColor>
        where TSource : IParameterizedCurve
    {
        private readonly TSource[] _sources;
        private readonly Func<PointXY, ParameterizedCurveProjection, TColor> _projectionToColor;

        public ParameterizedProjectionBasedLayer(
            IReadOnlyList<TSource> sources,
            Func<PointXY, ParameterizedCurveProjection, TColor> projectionToColor)
        {
            _sources = CopySources(sources);
            _projectionToColor = projectionToColor ?? throw new ArgumentNullException(nameof(projectionToColor));
        }

        public TColor Sample(PointXY point)
        {
            ParameterizedCurveProjection nearestProjection = _sources[0].ProjectWithParameter(point);

            for (int i = 1; i < _sources.Length; i++)
            {
                ParameterizedCurveProjection projection = _sources[i].ProjectWithParameter(point);
                if (projection.Distance < nearestProjection.Distance)
                    nearestProjection = projection;
            }

            return _projectionToColor(point, nearestProjection);
        }

        private static TSource[] CopySources(IReadOnlyList<TSource> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Count == 0)
                throw new ArgumentException("Parameterized curve collection must not be empty.", nameof(sources));

            var copy = new TSource[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                TSource source = sources[i];
                if (source is null)
                    throw new ArgumentException("Parameterized curve collection cannot contain null elements.", nameof(sources));

                copy[i] = source;
            }

            return copy;
        }
    }
}
