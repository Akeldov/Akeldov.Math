using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a mutable hex-indexed value map with spatial geometry.
    /// </summary>
    /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
    public class SpatialHexMap<TValue> : HexMap<TValue>, ISpatialHexMap<TValue>
    {
        /// <summary>
        /// Initializes a new spatial hex map with default values.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialHexMap(HexMapGeometry geometry)
            : base(geometry.Topology)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
        }

        /// <summary>
        /// Initializes a new spatial hex map that uses the specified array as its backing storage.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        /// <param name="values">
        /// The backing array. Its length must equal the number of cells in <paramref name="geometry"/>.
        /// Values must be stored in row-major order: X advances first, and the value at coordinates
        /// <c>(x, y)</c> is stored at <c>y * geometry.Topology.Resolution.X + x</c>.
        /// </param>
        /// <remarks>
        /// <b>Ownership warning:</b> the array is retained by the map and is not copied. The caller and
        /// the map share the same mutable storage, so changes made through either one are visible through
        /// the other. Do not reuse or modify the array independently when exclusive map ownership is required.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="values"/> does not match the geometry topology cell count.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialHexMap(HexMapGeometry geometry, TValue[] values)
            : base(geometry.Topology, values)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
        }

        /// <summary>
        /// Gets the spatial geometry of the hex map. Its topology equals <see cref="HexMap{TValue}.Topology"/>.
        /// </summary>
        public HexMapGeometry Geometry { get; }
    }
}
