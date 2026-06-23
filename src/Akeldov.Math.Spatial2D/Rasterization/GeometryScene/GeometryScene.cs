using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Describes ordered geometry layers that can be sampled into a single color buffer.
    /// </summary>
    /// <typeparam name="TColor">The color value type produced by scene layers.</typeparam>
    /// <remarks>
    /// Layers are sampled in insertion order and composited with each layer's blend function.
    /// Unsigned distance layers are suitable for points and open curves. Signed distance layers
    /// are suitable for contours and regions with inside/outside semantics.
    /// </remarks>
    public sealed class GeometryScene<TColor>
    {
        private readonly List<IGeometrySceneLayer<TColor>> _layers;
        private readonly IReadOnlyList<IGeometrySceneLayer<TColor>> _readOnlyLayers;
        private readonly Func<TColor, TColor, TColor> _defaultLayerBlend;
        private readonly Func<TColor, float, TColor> _applyCoverage;

        /// <summary>
        /// Initializes a new geometry scene with the default color as its background.
        /// </summary>
        /// <param name="blend">The default function assigned to layers created by this scene.</param>
        /// <param name="applyCoverage">The function that applies normalized coverage to a layer color.</param>
        public GeometryScene(
            Func<TColor, TColor, TColor> blend,
            Func<TColor, float, TColor> applyCoverage)
            : this(default!, blend, applyCoverage)
        {
        }

        /// <summary>
        /// Initializes a new geometry scene with the specified background color.
        /// </summary>
        /// <param name="backgroundColor">The color used before any layer is composited.</param>
        /// <param name="blend">The default function assigned to layers created by this scene.</param>
        /// <param name="applyCoverage">The function that applies normalized coverage to a layer color.</param>
        public GeometryScene(
            TColor backgroundColor,
            Func<TColor, TColor, TColor> blend,
            Func<TColor, float, TColor> applyCoverage)
        {
            if (blend == null)
                throw new ArgumentNullException(nameof(blend));

            if (applyCoverage == null)
                throw new ArgumentNullException(nameof(applyCoverage));

            BackgroundColor = backgroundColor;
            _defaultLayerBlend = blend;
            _applyCoverage = applyCoverage;
            _layers = new List<IGeometrySceneLayer<TColor>>();
            _readOnlyLayers = _layers.AsReadOnly();
        }

        /// <summary>
        /// Gets the color used before any layer is composited.
        /// </summary>
        public TColor BackgroundColor { get; }

        /// <summary>
        /// Gets the read-only structural view of scene layers in compositing order.
        /// </summary>
        public IReadOnlyList<IGeometrySceneLayer<TColor>> Layers => _readOnlyLayers;

        /// <summary>
        /// Adds a custom geometry scene layer.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> AddLayer(IGeometrySceneLayer<TColor> layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            _layers.Add(layer);
            return this;
        }

        /// <summary>
        /// Adds an unsigned point-distance layer.
        /// </summary>
        /// <param name="source">The distance provider to sample.</param>
        /// <param name="distanceToColor">The function that maps unsigned distance in world coordinate units to a color.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Distance(
            IPointDistanceProvider source,
            Func<float, TColor> distanceToColor)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (distanceToColor == null)
                throw new ArgumentNullException(nameof(distanceToColor));

            return AddLayer(new DistanceGeometrySceneLayer<TColor>(source, distanceToColor, _defaultLayerBlend));
        }

        /// <summary>
        /// Adds a signed point-distance layer.
        /// </summary>
        /// <param name="source">The signed distance provider to sample.</param>
        /// <param name="signedDistanceToColor">The function that maps signed distance in world coordinate units to a color.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> SignedDistance(
            ISignedPointDistanceProvider source,
            Func<float, TColor> signedDistanceToColor)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (signedDistanceToColor == null)
                throw new ArgumentNullException(nameof(signedDistanceToColor));

            return AddLayer(new SignedDistanceGeometrySceneLayer<TColor>(source, signedDistanceToColor, _defaultLayerBlend));
        }

        /// <summary>
        /// Adds a hard-edged stroke around an unsigned point-distance provider.
        /// </summary>
        /// <param name="source">The distance provider to stroke.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="width">The positive stroke width in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Stroke(IPointDistanceProvider source, TColor color, float width)
        {
            return Stroke(source, color, width, 0f);
        }

        /// <summary>
        /// Adds a stroke around an unsigned point-distance provider.
        /// </summary>
        /// <param name="source">The distance provider to stroke.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="width">The positive stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke, in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Stroke(
            IPointDistanceProvider source,
            TColor color,
            float width,
            float edgeFalloff)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            GeometrySceneValidation.ValidatePositiveFinite(width, nameof(width), "Stroke width must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Stroke edge falloff must be finite and non-negative.");

            return AddLayer(new StrokeGeometrySceneLayer<TColor>(source, color, width, edgeFalloff, _applyCoverage, _defaultLayerBlend));
        }

        /// <summary>
        /// Adds a hard-edged filled signed-distance provider layer.
        /// </summary>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Fill(ISignedPointDistanceProvider source, TColor color)
        {
            return Fill(source, color, 0f);
        }

        /// <summary>
        /// Adds a filled signed-distance provider layer.
        /// </summary>
        /// <param name="source">The signed distance provider to fill.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the filled boundary, in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Fill(ISignedPointDistanceProvider source, TColor color, float edgeFalloff)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Fill edge falloff must be finite and non-negative.");

            return AddLayer(new FillGeometrySceneLayer<TColor>(source, color, edgeFalloff, _applyCoverage, _defaultLayerBlend));
        }

        /// <summary>
        /// Adds a hard-edged point marker layer.
        /// </summary>
        /// <param name="point">The finite point marker center.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Point(PointXY point, TColor color, float radius)
        {
            return Point(point, color, radius, 0f);
        }

        /// <summary>
        /// Adds a point marker layer.
        /// </summary>
        /// <param name="point">The finite point marker center.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the marker, in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Point(PointXY point, TColor color, float radius, float edgeFalloff)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point marker coordinates must be finite.");

            GeometrySceneValidation.ValidatePositiveFinite(radius, nameof(radius), "Point marker radius must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Point marker edge falloff must be finite and non-negative.");

            return AddLayer(new PointMarkerCollectionGeometrySceneLayer<TColor>(
                new[] { point },
                color,
                radius,
                edgeFalloff,
                _applyCoverage,
                _defaultLayerBlend));
        }

        /// <summary>
        /// Adds a hard-edged point marker layer for a copied set of finite points.
        /// </summary>
        /// <param name="points">The finite point marker centers. The list is validated and copied when the layer is added.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Point(IReadOnlyList<PointXY> points, TColor color, float radius)
        {
            return Point(points, color, radius, 0f);
        }

        /// <summary>
        /// Adds a point marker layer for a copied set of finite points.
        /// </summary>
        /// <param name="points">The finite point marker centers. The list is validated and copied when the layer is added.</param>
        /// <param name="color">The marker color.</param>
        /// <param name="radius">The positive marker radius in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside each marker, in world coordinate units.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> Point(
            IReadOnlyList<PointXY> points,
            TColor color,
            float radius,
            float edgeFalloff)
        {
            PointXY[] pointCopy = CopyPointMarkers(points, nameof(points));

            GeometrySceneValidation.ValidatePositiveFinite(radius, nameof(radius), "Point marker radius must be finite and positive.");
            GeometrySceneValidation.ValidateNonNegativeFinite(edgeFalloff, nameof(edgeFalloff), "Point marker edge falloff must be finite and non-negative.");

            return AddLayer(new PointMarkerCollectionGeometrySceneLayer<TColor>(pointCopy, color, radius, edgeFalloff, _applyCoverage, _defaultLayerBlend));
        }

        /// <summary>
        /// Rasterizes this scene into a new color buffer on the specified grid.
        /// </summary>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <returns>A new mutable color buffer owned by the caller.</returns>
        public TColor[] RasterizeValues(RasterGrid grid)
        {
            GeometrySceneValidation.ValidateGrid(grid);

            var values = new TColor[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                int valueIndex = y * grid.Resolution.X;

                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    TColor color = BackgroundColor;

                    for (int layerIndex = 0; layerIndex < _layers.Count; layerIndex++)
                    {
                        IGeometrySceneLayer<TColor> layer = _layers[layerIndex];
                        color = layer.Blend(color, layer.Sample(point));
                    }

                    values[valueIndex++] = color;
                }
            }

            return values;
        }

        /// <summary>
        /// Rasterizes this scene on the specified grid and creates a raster artifact from the new color buffer.
        /// </summary>
        /// <typeparam name="TRaster">The raster artifact type to create.</typeparam>
        /// <param name="grid">The raster grid that describes the sampled region.</param>
        /// <param name="createRaster">The function that creates a raster artifact from the grid and new mutable color buffer.</param>
        /// <returns>The raster artifact created by <paramref name="createRaster"/>.</returns>
        public TRaster Rasterize<TRaster>(
            RasterGrid grid,
            Func<RasterGrid, TColor[], TRaster> createRaster)
        {
            if (createRaster == null)
                throw new ArgumentNullException(nameof(createRaster));

            return createRaster(grid, RasterizeValues(grid));
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
