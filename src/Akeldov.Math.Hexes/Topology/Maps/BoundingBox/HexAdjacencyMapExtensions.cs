using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology.Maps.BoundingBox
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class HexAdjacencyMapExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexAdjacencyMap">The hexAdjacencyMap value.</param>
        /// <param name="hexRadius">The hexRadius value.</param>
        public static VectorXY GetBoundingBoxSize(this IndexSeptupletMap hexAdjacencyMap, float hexRadius)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var resolution = new VectorXYInt(hexAdjacencyMap.Width, hexAdjacencyMap.Height);
            return resolution.BoundingBox(
                hexRadius.ConvertHexRadiusToApothem(),
                hexRadius,
                hexAdjacencyMap.Layout);
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexAdjacencyMap">The hexAdjacencyMap value.</param>
        /// <param name="hexRadius">The hexRadius value.</param>
        public static VectorXY GetBoundingBoxSize(this IndexPartialSeptupletMap hexAdjacencyMap, float hexRadius)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var resolution = new VectorXYInt(hexAdjacencyMap.Width, hexAdjacencyMap.Height);
            return resolution.BoundingBox(
                hexRadius.ConvertHexRadiusToApothem(),
                hexRadius,
                hexAdjacencyMap.Layout);
        }
    }
}
