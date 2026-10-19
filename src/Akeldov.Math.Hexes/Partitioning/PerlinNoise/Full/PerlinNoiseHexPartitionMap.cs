using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>
    /// Stores territory-type identifiers assigned by Perlin-noise bands and the geometry used to sample them.
    /// </summary>
    /// <remarks>
    /// Cells are structural snapshots in target order, including empty types. A type may have disconnected
    /// components and holes. Inherited indexer setters change only assignments, not the retained cell snapshots.
    /// </remarks>
    public sealed class PerlinNoiseHexPartitionMap : SpatialHexPartitionMap<HexPartitionCell>
    {
        internal PerlinNoiseHexPartitionMap(HexMapGeometry geometry, HexPartitionCell[] cells)
            : base(geometry, cells)
        {
        }

        /// <summary>
        /// Gets the geometry used to sample the noise. Its topology equals this map's topology.
        /// </summary>
        public new HexMapGeometry Geometry => base.Geometry;
    }
}
