using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>
    /// Stores territory-type identifiers assigned by Perlin-noise bands and the geometry used to sample them.
    /// </summary>
    /// <remarks>
    /// Cells are structural snapshots in target order, including empty types. A type may have disconnected
    /// components and holes. Inherited indexer setters change only assignments, not the retained cell snapshots.
    /// </remarks>
    public sealed class PerlinNoiseHexPartitionMap : HexPartitionMap<HexPartitionCell>, ISpatialHexMap<int>
    {
        internal PerlinNoiseHexPartitionMap(HexMapGeometry geometry, HexPartitionCell[] cells)
            : base(geometry.Topology, cells)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");
            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
        }

        /// <summary>
        /// Gets the geometry used to sample the noise. Its topology equals this map's topology.
        /// </summary>
        public HexMapGeometry Geometry { get; }
    }
}
