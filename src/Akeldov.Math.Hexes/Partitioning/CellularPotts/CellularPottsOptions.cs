using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    /// <summary>
    /// Defines the temperature and energy coefficients for a Cellular Potts model.
    /// </summary>
    /// <remarks>
    /// The energy is J times the number of unlike, undirected neighbor pairs, plus
    /// lambda times the sum of (area - target area) squared over all cells.
    /// Areas are measured in hex counts; every shared edge has unit length.
    /// </remarks>
    public sealed class CellularPottsOptions
    {
        /// <summary>Initializes immutable Cellular Potts parameters.</summary>
        /// <param name="temperature">The finite, non-negative fluctuation temperature in energy units. Zero accepts only non-increasing energy changes.</param>
        /// <param name="boundaryEnergy">The finite, non-negative energy J per edge between different cells.</param>
        /// <param name="areaPenalty">The finite, non-negative quadratic area coefficient lambda, in energy units per squared hex count.</param>
        /// <param name="targetAreas">
        /// Optional target areas keyed by cell identifier, copied into a read-only snapshot.
        /// Values must be finite and between zero and <see cref="int.MaxValue"/> hexes.
        /// Missing identifiers use their initial area for each application of the algorithm.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">An energy parameter or target area is outside its documented range.</exception>
        /// <exception cref="ArgumentException">A target-area identifier is negative.</exception>
        public CellularPottsOptions(
            double temperature = 1d,
            double boundaryEnergy = 1d,
            double areaPenalty = 1d,
            IReadOnlyDictionary<int, double>? targetAreas = null)
        {
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

            Temperature = temperature;
            BoundaryEnergy = boundaryEnergy;
            AreaPenalty = areaPenalty;
            TargetAreas = new ReadOnlyDictionary<int, double>(copy);
        }

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
