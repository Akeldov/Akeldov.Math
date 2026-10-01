using System;

namespace Akeldov.Math.Hexes
{
    public static partial class IHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one erosion over each cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// only when the source cell itself and every existing neighbor are <see langword="true"/>.
        /// Neighbors outside the map domain are ignored, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Erode(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            BooleanHexMapErosionHelper.FillErodedValues(map, values);
            return new BoolHexMap(map.Topology, values);
        }
    }
}
