using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    /// <summary>
    /// Defines immutable duration, connectivity, temperature, and energy settings for a Cellular Potts model.
    /// </summary>
    /// <remarks>
    /// The energy is J times the number of unlike, undirected neighbor pairs, plus
    /// lambda times the sum of (area - target area) squared over all cells.
    /// Areas are measured in participating hex counts; every eligible shared edge has unit length.
    /// Masked applications omit excluded hexes and edges across region boundaries. With a region mask,
    /// area penalties and connectivity are evaluated independently for each cell in each region.
    /// </remarks>
    public sealed class CellularPottsParameters
    {
        /// <summary>Initializes immutable Cellular Potts parameters.</summary>
        /// <param name="stepCount">The non-negative number of steps; each step makes one copy attempt per participating hex, sampled with replacement.</param>
        /// <param name="temperature">The finite, non-negative fluctuation temperature in energy units. Zero accepts only non-increasing energy changes.</param>
        /// <param name="boundaryEnergy">The finite, non-negative energy J per edge between different cells.</param>
        /// <param name="areaPenalty">The finite, non-negative quadratic area coefficient lambda, in energy units per squared hex count.</param>
        /// <param name="targetAreas">
        /// Optional target areas keyed by cell identifier, copied into a read-only snapshot.
        /// Values must be finite and between zero and <see cref="int.MaxValue"/> hexes.
        /// Missing identifiers use their initial participating area for each application of the algorithm.
        /// With a region mask, each explicit value applies independently in every region where the cell is
        /// initially present; missing values use the cell's initial participating area in that region.
        /// </param>
        /// <param name="connectivity">The connectivity policy. Defaults to preserving each non-empty participating cell as one six-connected component within each region.</param>
        /// <exception cref="ArgumentOutOfRangeException">A step count, energy parameter, target area, or connectivity policy is outside its documented range.</exception>
        /// <exception cref="ArgumentException">A target-area identifier is negative.</exception>
        public CellularPottsParameters(
            int stepCount,
            double temperature = 1d,
            double boundaryEnergy = 1d,
            double areaPenalty = 1d,
            IReadOnlyDictionary<int, double>? targetAreas = null,
            CellularPottsConnectivity connectivity = CellularPottsConnectivity.Preserve)
        {
            if (stepCount < 0)
                throw new ArgumentOutOfRangeException(nameof(stepCount));
            if (connectivity != CellularPottsConnectivity.None && connectivity != CellularPottsConnectivity.Preserve)
                throw new ArgumentOutOfRangeException(nameof(connectivity));
            if (!double.IsFinite(temperature) || temperature < 0d)
                throw new ArgumentOutOfRangeException(nameof(temperature));
            if (!double.IsFinite(boundaryEnergy) || boundaryEnergy < 0d)
                throw new ArgumentOutOfRangeException(nameof(boundaryEnergy));
            if (!double.IsFinite(areaPenalty) || areaPenalty < 0d)
                throw new ArgumentOutOfRangeException(nameof(areaPenalty));

            var copy = new Dictionary<int, double>();
            if (targetAreas != null)
            {
                foreach (var pair in targetAreas)
                {
                    if (pair.Key < 0)
                        throw new ArgumentException("Target-area identifiers must be non-negative.", nameof(targetAreas));
                    if (!double.IsFinite(pair.Value) || pair.Value < 0d || pair.Value > int.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(targetAreas), "Target areas must be finite hex counts between zero and int.MaxValue.");
                    copy.Add(pair.Key, pair.Value);
                }
            }

            StepCount = stepCount;
            Connectivity = connectivity;
            Temperature = temperature;
            BoundaryEnergy = boundaryEnergy;
            AreaPenalty = areaPenalty;
            TargetAreas = new ReadOnlyDictionary<int, double>(copy);
        }

        /// <summary>Gets the number of steps, each making one copy attempt per participating hex.</summary>
        public int StepCount { get; }

        /// <summary>Gets the cell connectivity policy.</summary>
        public CellularPottsConnectivity Connectivity { get; }

        /// <summary>Gets the fluctuation temperature in energy units.</summary>
        public double Temperature { get; }

        /// <summary>Gets the energy per unit edge separating different cells.</summary>
        public double BoundaryEnergy { get; }

        /// <summary>Gets the quadratic area coefficient in energy units per squared hex count.</summary>
        public double AreaPenalty { get; }

        /// <summary>Gets the retained read-only snapshot of explicit target hex counts, keyed by cell identifier.</summary>
        public IReadOnlyDictionary<int, double> TargetAreas { get; }
    }
}
