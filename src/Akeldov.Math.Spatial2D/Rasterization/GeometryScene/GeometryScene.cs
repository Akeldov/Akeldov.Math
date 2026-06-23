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

        internal Func<TColor, TColor, TColor> DefaultLayerBlend => _defaultLayerBlend;

        internal Func<TColor, float, TColor> ApplyCoverage => _applyCoverage;

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
    }
}
