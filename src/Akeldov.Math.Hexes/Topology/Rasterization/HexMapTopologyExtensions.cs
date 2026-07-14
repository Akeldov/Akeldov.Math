using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides rasterization extension methods for hex map topology values.
    /// </summary>
    public static class HexMapTopologyExtensions
    {
        /// <summary>
        /// Rasterizes unique hex edge segments for the whole topology with the zero hex center at the coordinate origin.
        /// </summary>
        /// <param name="hexMapTopology">The topology to rasterize.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <param name="options">The rendering and output parameters.</param>
        /// <returns>A raster of the hex map edge segments.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this HexMapTopology hexMapTopology,
            float radius,
            HexMapTopologyRasterizationOptions options)
        {
            return hexMapTopology.Rasterize(
                radius,
                VectorXY.Zero,
                options);
        }

        /// <summary>
        /// Rasterizes unique hex edge segments for the whole topology with the specified zero hex center.
        /// </summary>
        /// <param name="hexMapTopology">The topology to rasterize.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="options">The rendering and output parameters.</param>
        /// <returns>A raster of the hex map edge segments.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this HexMapTopology hexMapTopology,
            float radius,
            VectorXY origin,
            HexMapTopologyRasterizationOptions options)
        {
            var hexMapGeometry = new HexMapGeometry(hexMapTopology.Resolution.X, hexMapTopology.Resolution.Y, origin, radius, hexMapTopology.Layout);
            var rasterGeometry = hexMapGeometry.ToRasterGeometry(options.PixelsPerApothem, options.Margin);

            var res = hexMapGeometry
                .ToHexEdgeSegments()
                .Rasterize(
                    options.CurveWidth,
                    options.FadeDistance,
                    options.CurveColor,
                    options.BackgroundColor,
                    rasterGeometry);

            return res;
        }

        /// <summary>Rasterizes a topology with XY index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(this HexMapTopology topology, float radius,
            HexMapTopologyRasterizationOptions options, HexMapTopologyXYLabelsRasterizationOptions labels) =>
            topology.Rasterize(radius, VectorXY.Zero, options, labels);

        /// <summary>Rasterizes a topology at an origin with XY index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(this HexMapTopology topology, float radius, VectorXY origin,
            HexMapTopologyRasterizationOptions options, HexMapTopologyXYLabelsRasterizationOptions labels)
        {
            var geometry = new HexMapGeometry(topology, origin, radius);
            SpatialRaster<Gray8BitColor> raster = topology.Rasterize(radius, origin, options);
            AddLabels(raster, new HexCenterMap(geometry), labels.Font, labels.FontSize, labels.Color,
                labels.EdgeFalloff, labels.Offset, index => $"({index.X}, {index.Y})");
            return raster;
        }

        /// <summary>Rasterizes a topology with QRS index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(this HexMapTopology topology, float radius,
            HexMapTopologyRasterizationOptions options, HexMapTopologyQRSLabelsRasterizationOptions labels) =>
            topology.Rasterize(radius, VectorXY.Zero, options, labels);

        /// <summary>Rasterizes a topology at an origin with QRS index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(this HexMapTopology topology, float radius, VectorXY origin,
            HexMapTopologyRasterizationOptions options, HexMapTopologyQRSLabelsRasterizationOptions labels)
        {
            var geometry = new HexMapGeometry(topology, origin, radius);
            SpatialRaster<Gray8BitColor> raster = topology.Rasterize(radius, origin, options);
            AddLabels(raster, new HexCenterMap(geometry), labels.Font, labels.FontSize, labels.Color,
                labels.EdgeFalloff, labels.Offset, index =>
                {
                    VectorQRSInt qrs = index.ToQRSIndex(topology.Layout);
                    return $"({qrs.Q}, {qrs.R}, {qrs.S})";
                });
            return raster;
        }

        /// <summary>Rasterizes a topology with both XY and QRS index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this HexMapTopology topology,
            float radius,
            HexMapTopologyRasterizationOptions options,
            HexMapTopologyXYLabelsRasterizationOptions xyLabels,
            HexMapTopologyQRSLabelsRasterizationOptions qrsLabels) =>
            topology.Rasterize(radius, VectorXY.Zero, options, xyLabels, qrsLabels);

        /// <summary>Rasterizes a topology at an origin with both XY and QRS index labels.</summary>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this HexMapTopology topology,
            float radius,
            VectorXY origin,
            HexMapTopologyRasterizationOptions options,
            HexMapTopologyXYLabelsRasterizationOptions xyLabels,
            HexMapTopologyQRSLabelsRasterizationOptions qrsLabels)
        {
            var geometry = new HexMapGeometry(topology, origin, radius);
            var centers = new HexCenterMap(geometry);
            SpatialRaster<Gray8BitColor> raster = topology.Rasterize(radius, origin, options);

            AddLabels(raster, centers, xyLabels.Font, xyLabels.FontSize, xyLabels.Color,
                xyLabels.EdgeFalloff, xyLabels.Offset, index => $"({index.X}, {index.Y})");
            AddLabels(raster, centers, qrsLabels.Font, qrsLabels.FontSize, qrsLabels.Color,
                qrsLabels.EdgeFalloff, qrsLabels.Offset, index =>
                {
                    VectorQRSInt qrs = index.ToQRSIndex(topology.Layout);
                    return $"({qrs.Q}, {qrs.R}, {qrs.S})";
                });

            return raster;
        }

        private static void AddLabels(SpatialRaster<Gray8BitColor> target, HexCenterMap centers, TrueTypeFont font,
            float fontSize, Gray8BitColor color, float edgeFalloff, VectorXY offset, Func<VectorXYInt, string> getLabel)
        {
            var texts = new List<TextSignedDistanceProvider>(centers.Topology.Count);
            for (int y = 0; y < centers.Topology.Resolution.Y; y++)
            for (int x = 0; x < centers.Topology.Resolution.X; x++)
            {
                var index = new VectorXYInt(x, y);
                texts.Add(TextSignedDistanceProvider.Create(
                    font,
                    getLabel(index),
                    centers[index] + offset,
                    fontSize,
                    new TextLayoutOptions { Anchor = TextAnchor.Center, UseKerning = false }));
            }

            SpatialRaster<float> label = new GeometryScene<float>(float.PositiveInfinity, MathF.Min)
                .AddTextLayer(texts, distance => distance, edgeFalloff)
                .Rasterize(target.Grid);

            for (int i = 0; i < label.Values.Length; i++)
            {
                float distance = label.Values[i];
                float coverage = distance <= 0f ? 1f :
                    edgeFalloff > 0f && distance < edgeFalloff ? 1f - distance / edgeFalloff : 0f;
                if (coverage <= 0f)
                    continue;

                Gray8BitColor current = target.Values[i];
                target.Values[i] = Gray8BitColor.Blend(current, color, coverage);
            }
        }
    }
}
