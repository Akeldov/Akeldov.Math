using System;

namespace Akeldov.Math.Hexes
{
    public static partial class ISpatialHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one erosion over each spatial cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// A result cell is <see langword="true"/> only when the source cell itself and every existing
        /// neighbor are <see langword="true"/>. Neighbors outside the map domain are ignored, and the
        /// source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Erode(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            BooleanHexMapErosionHelper.FillErodedValues(map, values);
            return new SpatialBoolHexMap(map.Geometry, values);
        }
    }
}
