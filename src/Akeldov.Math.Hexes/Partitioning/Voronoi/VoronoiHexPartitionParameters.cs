using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using System;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>Defines immutable relaxation and cell-handling settings for full and partial hex Voronoi partitions.</summary>
    public sealed class VoronoiHexPartitionParameters
    {
        /// <summary>Initializes hex Voronoi partition settings.</summary>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Zero disables relaxation.</param>
        /// <param name="emptyCellPolicy">The policy for empty cells in the final partition. Defaults to retaining them.</param>
        /// <param name="exclavePolicy">The policy for disconnected cell components after relaxation, preserving region boundaries.</param>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative or a policy is invalid.</exception>
        public VoronoiHexPartitionParameters(
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));
            if (emptyCellPolicy != EmptyCellPolicy.ThrowException && emptyCellPolicy != EmptyCellPolicy.Exclude && emptyCellPolicy != EmptyCellPolicy.LeaveAsIs)
                throw new ArgumentOutOfRangeException(nameof(emptyCellPolicy));
            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

            RelaxationIterations = relaxationIterations;
            EmptyCellPolicy = emptyCellPolicy;
            ExclavePolicy = exclavePolicy;
        }

        /// <summary>Gets the number of centroid relaxation iterations.</summary>
        public int RelaxationIterations { get; }

        /// <summary>Gets the policy for empty cells in the final partition.</summary>
        public EmptyCellPolicy EmptyCellPolicy { get; }

        /// <summary>Gets the policy for disconnected components in the final partition.</summary>
        public ExclavePolicy ExclavePolicy { get; }
    }
}
