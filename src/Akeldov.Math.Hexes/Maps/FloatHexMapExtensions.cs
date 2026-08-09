using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides conversions to mutable floating-point hex maps.
    /// </summary>
    public static class FloatHexMapExtensions
    {
        /// <summary>
        /// Creates an independent mutable copy of the specified floating-point hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static FloatHexMap ToFloatHexMap(this IHexMap<float> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new float[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new FloatHexMap(map.Topology, values);
        }
    }
}
