using System;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    public static partial class IHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one morphological closing: dilation followed by erosion.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. Both passes use the source domain,
        /// ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Close(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                BooleanHexMapDilationHelper.FillDilatedValues(map, scratch);
                BooleanHexMapErosionHelper.FillErodedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new BoolHexMap(map.Topology, values);
        }
    }
}
