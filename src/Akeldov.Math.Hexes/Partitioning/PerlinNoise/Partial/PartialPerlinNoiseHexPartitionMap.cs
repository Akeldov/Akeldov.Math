using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Stores nullable Perlin-noise territory identifiers and the geometry used to sample them.</summary>
    /// <remarks>
    /// Excluded hexes initially contain null. Cells are read-only structural snapshots in target order,
    /// including empty types, and may have disconnected components and holes. The source participation
    /// mask is not retained. Inherited indexer setters change only assignments, not the cell snapshots.
    /// </remarks>
    public sealed class PartialPerlinNoiseHexPartitionMap : PartialHexPartitionMap<HexPartitionCell>, ISpatialHexMap<int?>
    {
        internal PartialPerlinNoiseHexPartitionMap(HexMapGeometry geometry, HexPartitionCell[] cells)
            : base(geometry.Topology, cells)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");
            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
        }

        /// <summary>Gets the geometry used to sample the noise. Its topology equals this map's topology.</summary>
        public HexMapGeometry Geometry { get; }
    }
}
