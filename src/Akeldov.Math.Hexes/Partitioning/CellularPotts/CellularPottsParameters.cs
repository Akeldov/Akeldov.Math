using System;

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
    /// Energy changes and acceptance probabilities are calculated in double precision.
    /// </remarks>
    public sealed class CellularPottsParameters
    {
        /// <summary>Initializes immutable Cellular Potts parameters.</summary>
        /// <param name="stepCount">The non-negative number of steps; each step makes one copy attempt per participating hex, sampled with replacement.</param>
        /// <param name="temperature">The finite, non-negative fluctuation temperature in energy units. Zero accepts only non-increasing energy changes.</param>
        /// <param name="boundaryEnergy">The finite, non-negative energy J per edge between different cells.</param>
        /// <param name="areaPenalty">The finite, non-negative quadratic area coefficient lambda, in energy units per squared hex count.</param>
        /// <param name="connectivity">The connectivity policy. Defaults to preserving each non-empty participating cell as one six-connected component within each region.</param>
        /// <exception cref="ArgumentOutOfRangeException">A step count, energy parameter, or connectivity policy is outside its documented range.</exception>
        public CellularPottsParameters(
            int stepCount,
            float temperature = 1f,
            float boundaryEnergy = 1f,
            float areaPenalty = 1f,
            CellularPottsConnectivity connectivity = CellularPottsConnectivity.Preserve)
        {
            if (stepCount < 0)
                throw new ArgumentOutOfRangeException(nameof(stepCount));
            if (connectivity != CellularPottsConnectivity.None && connectivity != CellularPottsConnectivity.Preserve)
                throw new ArgumentOutOfRangeException(nameof(connectivity));
            if (!float.IsFinite(temperature) || temperature < 0f)
                throw new ArgumentOutOfRangeException(nameof(temperature));
            if (!float.IsFinite(boundaryEnergy) || boundaryEnergy < 0f)
                throw new ArgumentOutOfRangeException(nameof(boundaryEnergy));
            if (!float.IsFinite(areaPenalty) || areaPenalty < 0f)
                throw new ArgumentOutOfRangeException(nameof(areaPenalty));

            StepCount = stepCount;
            Connectivity = connectivity;
            Temperature = temperature;
            BoundaryEnergy = boundaryEnergy;
            AreaPenalty = areaPenalty;
        }

        /// <summary>Gets the number of steps, each making one copy attempt per participating hex.</summary>
        public int StepCount { get; }

        /// <summary>Gets the cell connectivity policy.</summary>
        public CellularPottsConnectivity Connectivity { get; }

        /// <summary>Gets the fluctuation temperature in energy units.</summary>
        public float Temperature { get; }

        /// <summary>Gets the energy per unit edge separating different cells.</summary>
        public float BoundaryEnergy { get; }

        /// <summary>Gets the quadratic area coefficient in energy units per squared hex count.</summary>
        public float AreaPenalty { get; }
    }
}
