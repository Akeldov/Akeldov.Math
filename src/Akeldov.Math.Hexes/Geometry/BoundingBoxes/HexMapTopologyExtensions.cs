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
        /// <param name="topology">The hex map topology.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <returns>The axis-aligned rectangle that contains all hexes in the map.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="origin"/> contains a non-finite component,
        /// when <paramref name="radius"/> is not finite and positive,
        /// when <paramref name="topology"/> has empty dimensions, or when its layout is unsupported.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle GetBoundingBox(this HexMapTopology topology, VectorXY origin, float radius)
        {
            return new HexMapGeometry(topology, origin, radius).GetBoundingBox();
        }
    }
}
