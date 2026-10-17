using System;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>Defines immutable relaxation and empty-cell settings for Voronoi item partitioning.</summary>
    public sealed class VoronoiPartitionParameters
    {
        /// <summary>Initializes Voronoi item partition settings.</summary>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Zero disables relaxation.</param>
        /// <param name="emptyCellPolicy">The policy for empty partitions after relaxation. Defaults to throwing an exception.</param>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative or the policy is invalid.</exception>
        public VoronoiPartitionParameters(
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.ThrowException)
        {
            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));
            if (emptyCellPolicy != EmptyCellPolicy.ThrowException && emptyCellPolicy != EmptyCellPolicy.Exclude && emptyCellPolicy != EmptyCellPolicy.LeaveAsIs)
                throw new ArgumentOutOfRangeException(nameof(emptyCellPolicy));

            RelaxationIterations = relaxationIterations;
            EmptyCellPolicy = emptyCellPolicy;
        }

        /// <summary>Gets the number of centroid relaxation iterations.</summary>
        public int RelaxationIterations { get; }

        /// <summary>Gets the policy for empty partitions after relaxation.</summary>
        public EmptyCellPolicy EmptyCellPolicy { get; }
    }
}
