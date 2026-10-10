using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialIntHexMap
    {
        /// <summary>
        /// Creates a generic spatial facade sharing the source map's geometry and backing array.
        /// </summary>
        /// <param name="map">The source map, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable spatial facade sharing the source map's values, or <see langword="null"/> if
        /// <paramref name="map"/> is null. Changes through either map are visible through the other.
        /// </returns>
        /// <remarks>
        /// The conversion takes constant time and allocates only the facade object. Values are not copied.
        /// </remarks>
        [return: NotNullIfNotNull("map")]
        public static implicit operator SpatialHexMap<int>?(SpatialIntHexMap? map)
            => map is null ? null : new SpatialHexMap<int>(map.Geometry, map.BackingValues);

        /// <summary>
        /// Creates a specialized spatial integer facade sharing the source map's geometry and backing array.
        /// </summary>
        /// <param name="map">The source map, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable spatial integer facade sharing the source map's values, or <see langword="null"/>
        /// if <paramref name="map"/> is null. Changes through either map are visible through the other.
        /// </returns>
        /// <remarks>
        /// The conversion takes constant time and allocates only the facade object. Values are not copied.
        /// </remarks>
        [return: NotNullIfNotNull("map")]
        public static implicit operator SpatialIntHexMap?(SpatialHexMap<int>? map)
            => map is null ? null : new SpatialIntHexMap(map.Geometry, map.BackingValues);
    }
}
