using Akeldov.Math.Spatial2D.Imaging;
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
        private readonly TColor _backgroundColor;
        private readonly List<LayerPass<TColor>> _layerPasses;

        private readonly Func<TColor, TColor, TColor> _defaultLayerBlend;

        /// <summary>
        /// Initializes a new geometry scene with the default color as its background.
        /// </summary>
        /// <param name="defaultLayerBlend">The default function assigned to layers created by this scene.</param>
        public GeometryScene(
            Func<TColor, TColor, TColor> defaultLayerBlend)
            : this(default!, defaultLayerBlend)
        {
        }

        /// <summary>
        /// Initializes a new geometry scene with the specified background color.
        /// </summary>
        /// <param name="backgroundColor">The color used before any layer is composited.</param>
        /// <param name="defaultLayerBlend">The default function assigned to layers created by this scene.</param>
        public GeometryScene(
            TColor backgroundColor,
            Func<TColor, TColor, TColor> defaultLayerBlend)
        {
            if (defaultLayerBlend == null)
                throw new ArgumentNullException(nameof(defaultLayerBlend));

            _backgroundColor = backgroundColor;
            _defaultLayerBlend = defaultLayerBlend;
            _layerPasses = new List<LayerPass<TColor>>();
        }

        /// <summary>
        /// Adds a layer using the scene's default blend function.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> AddLayer(IGeometrySceneLayer<TColor> layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            _layerPasses.Add(new LayerPass<TColor>(layer, _defaultLayerBlend));
            return this;
        }

        /// <summary>
        /// Adds a layer using the specified blend function.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        /// <param name="blend">The function that composites the current scene color with the layer color.</param>
        /// <returns>This scene.</returns>
        public GeometryScene<TColor> AddLayer(IGeometrySceneLayer<TColor> layer, Func<TColor, TColor, TColor> blend)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            if (blend == null)
                throw new ArgumentNullException(nameof(blend));

            _layerPasses.Add(new LayerPass<TColor>(layer, blend));
            return this;
        }

        /// <summary>
        /// Samples all scene layers into a raster on the specified grid.
        /// </summary>
        /// <param name="grid">The grid that defines raster bounds, resolution, and cell centers.</param>
        /// <returns>A raster whose value array is new, mutable, and owned by the caller.</returns>
        public Raster<TColor> Rasterize(SpatialRasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");

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
                    TColor color = _backgroundColor;

                    for (int layerIndex = 0; layerIndex < _layerPasses.Count; layerIndex++)
                    {
                        var layerPass = _layerPasses[layerIndex];
                        var layer = layerPass.Layer;
                        var blend = layerPass.Blend;

                        color = blend(color, layer.Sample(point));
                    }

                    values[valueIndex++] = color;
                }
            }

            var raster = new Raster<TColor>(grid, values);
            return raster;
        }
    }
}
