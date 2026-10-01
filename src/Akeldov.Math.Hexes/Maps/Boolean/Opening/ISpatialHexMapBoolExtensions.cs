using System;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    public static partial class ISpatialHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one morphological opening to a spatial map: erosion followed by dilation.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// Both passes use the source domain, ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Open(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                BooleanHexMapErosionHelper.FillErodedValues(map, scratch);
                BooleanHexMapDilationHelper.FillDilatedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new SpatialBoolHexMap(map.Geometry, values);
        }
    }
}
