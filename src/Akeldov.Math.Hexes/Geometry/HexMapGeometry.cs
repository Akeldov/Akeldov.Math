using Akeldov.Math.Spatial2D;
using System;
using System.Globalization;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Describes the dimensions, layout, origin, and size of a rectangular hex map.
    /// </summary>
    public readonly struct HexMapGeometry : IEquatable<HexMapGeometry>
    {
        /// <summary>
        /// Initializes a new hex map geometry from a radius value.
        /// </summary>
        /// <param name="width">The map width in hexes.</param>
        /// <param name="height">The map height in hexes.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <param name="layout">The hex layout used by the map.</param>
        public HexMapGeometry(int width, int height, VectorXY origin, float radius, Layout layout)
            : this(new HexMapTopology(width, height, layout), origin, radius)
        {
        }

        /// <summary>
        /// Initializes a new hex map geometry from a radius value.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="origin"/> contains a non-finite component, or
        /// when <paramref name="radius"/> is not finite and positive.
        /// </exception>
        public HexMapGeometry(HexMapTopology topology, VectorXY origin, float radius)
        {
            if (!origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(origin), origin, "Hex map origin components must be finite.");

            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            Topology = topology;
            Origin = origin;
            Radius = radius;
            Apothem = Constants.Radius2Apothem * radius;
        }

        /// <summary>
        /// Initializes a new hex map geometry from a radius value and the default zero-hex origin.
        /// </summary>
        /// <param name="width">The map width in hexes.</param>
        /// <param name="height">The map height in hexes.</param>
        /// <param name="radius">The hex radius. The unit is the coordinate-space unit.</param>
        /// <param name="layout">The hex layout used by the map.</param>
        public HexMapGeometry(int width, int height, float radius, Layout layout)
            : this(new HexMapTopology(width, height, layout), radius)
        {
        }

        /// <summary>
        /// Initializes a new hex map geometry from a radius value and the default zero-hex origin.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        /// <param name="radius">The hex radius. The unit is the coordinate-space unit.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="radius"/> is not finite and positive.
        /// </exception>
        public HexMapGeometry(HexMapTopology topology, float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            float apothem = Constants.Radius2Apothem * radius;

            Topology = topology;
            Origin = GetDefaultOrigin(radius, topology.Layout);
            Radius = radius;
            Apothem = apothem;
        }

        /// <summary>
        /// Gets the map topology.
        /// </summary>
        public HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the center of the zero hex.
        /// </summary>
        public VectorXY Origin { get; }

        /// <summary>
        /// Gets the hex apothem. The unit is the coordinate-space unit.
        /// </summary>
        public float Apothem { get; }

        /// <summary>
        /// Gets the hex radius. The unit is the coordinate-space unit.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Indicates whether this geometry has the same topology, origin, and radius as another geometry.
        /// </summary>
        /// <param name="other">The geometry to compare with this geometry.</param>
        /// <returns><see langword="true"/> if both geometries are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(HexMapGeometry other) =>
            Topology.Equals(other.Topology) &&
            Origin.Equals(other.Origin) &&
            Radius.Equals(other.Radius);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is HexMapGeometry other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Topology, Origin, Radius);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "HexMapGeometry(topology: {0}, origin: {1}, radius: {2})",
                Topology,
                Origin,
                Radius);

        /// <summary>
        /// Deconstructs this geometry into its topology, origin, and radius.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        public void Deconstruct(out HexMapTopology topology, out VectorXY origin, out float radius)
        {
            topology = Topology;
            origin = Origin;
            radius = Radius;
        }

        /// <summary>
        /// Indicates whether two geometries are equal.
        /// </summary>
        /// <param name="left">The first geometry.</param>
        /// <param name="right">The second geometry.</param>
        /// <returns><see langword="true"/> if both geometries are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(HexMapGeometry left, HexMapGeometry right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two geometries are different.
        /// </summary>
        /// <param name="left">The first geometry.</param>
        /// <param name="right">The second geometry.</param>
        /// <returns><see langword="true"/> if the geometries differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(HexMapGeometry left, HexMapGeometry right) => !left.Equals(right);

        private static VectorXY GetDefaultOrigin(float radius, Layout layout)
        {
            float apothem = Constants.Radius2Apothem * radius;

            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(apothem, radius);
                case Layout.EvenR:
                    return new VectorXY(3f * apothem, radius);
                case Layout.OddQ:
                    return new VectorXY(radius, apothem);
                case Layout.EvenQ:
                    return new VectorXY(radius, 3f * apothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
