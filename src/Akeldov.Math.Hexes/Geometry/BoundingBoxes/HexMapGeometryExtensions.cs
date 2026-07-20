using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides bounding-box extension methods for hex map geometry and topology.
    /// </summary>
    public static partial class HexMapGeometryExtensions
    {
        /// <summary>
        /// Returns the axis-aligned bounding box of the whole hex map as a rectangle.
        /// </summary>
        /// <param name="geometry">The hex map geometry.</param>
        /// <returns>The axis-aligned rectangle that contains all hexes in the map.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> has a non-finite origin, non-positive radius, empty dimensions,
        /// or an unsupported layout.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle GetBoundingBox(this HexMapGeometry geometry)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            VectorXY size = geometry.GetBoundingBoxSize();
            float hexRadius = geometry.Radius;

            float minX;
            float minY;

            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                    minX = geometry.Origin.X - geometry.Apothem;
                    minY = geometry.Origin.Y - hexRadius;
                    break;
                case Layout.EvenR:
                    minX = geometry.Origin.X - geometry.Apothem * (geometry.Topology.Resolution.Y == 1 ? 1f : 2f);
                    minY = geometry.Origin.Y - hexRadius;
                    break;
                case Layout.OddQ:
                    minX = geometry.Origin.X - hexRadius;
                    minY = geometry.Origin.Y - geometry.Apothem;
                    break;
                case Layout.EvenQ:
                    minX = geometry.Origin.X - hexRadius;
                    minY = geometry.Origin.Y - geometry.Apothem * (geometry.Topology.Resolution.X == 1 ? 1f : 2f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry layout is not supported.");
            }

            return new Rectangle(
                new PointXY(minX, minY),
                new PointXY(minX + size.X, minY + size.Y));
        }

        /// <summary>
        /// Returns the size of the axis-aligned bounding box of the whole hex map.
        /// </summary>
        /// <param name="geometry">The hex map geometry.</param>
        /// <returns>The width and height of the axis-aligned bounding box.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> has a non-positive radius, empty dimensions,
        /// or an unsupported layout.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetBoundingBoxSize(this HexMapGeometry geometry)
        {
            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            if (geometry.Topology.Resolution.X <= 0 || geometry.Topology.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry dimensions must be positive.");

            VectorXYInt resolution = geometry.Topology.Resolution;
            switch (geometry.Topology.Layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return RowLayoutBoundingBox(resolution, geometry.Radius);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return ColumnLayoutBoundingBox(resolution, geometry.Radius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry layout is not supported.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXY RowLayoutBoundingBox(VectorXYInt dim, float hexRadius)
        {
            if (dim.X == 0 || dim.Y == 0)
                return new VectorXY(0, 0);

            float hexApothem = Constants.Radius2Apothem * hexRadius;
            var xMetricSize = hexApothem * 2f * dim.X + hexApothem * (dim.Y == 1 ? 0 : 1);
            var yMetricSize = hexRadius * 2f + hexRadius * 1.5f * (dim.Y - 1);
            return new VectorXY(xMetricSize, yMetricSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXY ColumnLayoutBoundingBox(VectorXYInt dim, float hexRadius)
        {
            if (dim.X == 0 || dim.Y == 0)
                return new VectorXY(0, 0);

            float hexApothem = Constants.Radius2Apothem * hexRadius;
            var xMetricSize = hexRadius * 2f + hexRadius * 1.5f * (dim.X - 1);
            var yMetricSize = hexApothem * 2f * dim.Y + hexApothem * (dim.X == 1 ? 0 : 1);
            return new VectorXY(xMetricSize, yMetricSize);
        }
    }
}
