namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    /// <summary>Specifies whether Cellular Potts evolution must preserve cell connectivity.</summary>
    public enum CellularPottsConnectivity
    {
        /// <summary>
        /// Copies a random neighboring hex's identifier without connectivity checks.
        /// Disconnected input cells, fragmentation, hole formation, and cell disappearance are allowed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Copies a random neighboring hex's identifier only if the donor's remaining neighbors
        /// are connected within the six-hex ring. Non-empty input cells must be six-connected.
        /// This constant-time check preserves non-empty cells but may reject globally safe moves
        /// whose connecting paths leave the ring. Holes are allowed.
        /// </summary>
        Preserve = 1
    }
}
