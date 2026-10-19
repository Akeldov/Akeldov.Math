using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Stores nullable Perlin-noise territory identifiers and the geometry used to sample them.</summary>
    /// <remarks>
    /// Excluded hexes initially contain null. Cells are read-only structural snapshots in target order,
    /// including empty types, and may have disconnected components and holes. The source participation
    /// mask is not retained. Inherited indexer setters change only assignments, not the cell snapshots.
    /// </remarks>
    public sealed class PartialPerlinNoiseHexPartitionMap : SpatialPartialHexPartitionMap<HexPartitionCell>
    {
        internal PartialPerlinNoiseHexPartitionMap(HexMapGeometry geometry, HexPartitionCell[] cells)
            : base(geometry, cells)
        {
        }

        /// <summary>Gets the geometry used to sample the noise. Its topology equals this map's topology.</summary>
        public new HexMapGeometry Geometry => base.Geometry;
    }
}
