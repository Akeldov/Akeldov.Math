using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides conversions to mutable integer hex maps.
    /// </summary>
    public static class IntHexMapExtensions
    {
        /// <summary>
        /// Creates an independent mutable copy of the specified integer hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static IntHexMap ToIntHexMap(this IHexMap<int> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new IntHexMap(map.Topology, values);
        }
    }
}
