using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    internal sealed class PerlinNoiseHexPartitioner
    {
        private readonly PerlinNoisePartitionTarget[] _targets;
        private readonly double _maximumShare;

        internal PerlinNoiseHexPartitioner(IReadOnlyList<PerlinNoisePartitionTarget> targets)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (targets.Count == 0)
                throw new ArgumentException("At least one territory target is required.", nameof(targets));

            _targets = new PerlinNoisePartitionTarget[targets.Count];
            var ids = new HashSet<int>();
            double maximumShare = 0d;
            for (int i = 0; i < _targets.Length; i++)
            {
                var target = targets[i];
                if (target == null)
                    throw new ArgumentException("Territory targets must be non-null.", nameof(targets));
                if (!ids.Add(target.Id))
                    throw new ArgumentException($"Duplicate territory identifier: {target.Id}.", nameof(targets));
                _targets[i] = target;
                maximumShare = System.Math.Max(maximumShare, target.AreaShare);
            }
            if (maximumShare == 0d)
                throw new ArgumentException("At least one area share must be positive.", nameof(targets));

            _maximumShare = maximumShare;
        }

        internal HexPartitionCell[] Partition(FloatHexMap noise)
        {
            var topology = noise.Topology;
            var thresholds = CreateThresholds(noise, _targets, _maximumShare);
            var groups = new List<VectorXYInt>[_targets.Length];
            for (int i = 0; i < groups.Length; i++)
                groups[i] = new List<VectorXYInt>();

            int width = topology.Resolution.X;
            for (int site = 0; site < topology.Count; site++)
            {
                int lower = 0;
                int upper = thresholds.Length;
                while (lower < upper)
                {
                    int middle = lower + (upper - lower) / 2;
                    if (noise[site] >= thresholds[middle])
                        lower = middle + 1;
                    else
                        upper = middle;
                }
                groups[lower].Add(new VectorXYInt(site % width, site / width));
            }

            var cells = new HexPartitionCell[_targets.Length];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = new HexPartitionCell(_targets[i].Id, groups[i]);
            return cells;
        }

        private static float[] CreateThresholds(FloatHexMap noise, PerlinNoisePartitionTarget[] targets, double maximumShare)
        {
            var thresholds = new float[targets.Length - 1];
            int count = noise.Topology.Count;
            if (count == 0 || thresholds.Length == 0)
                return thresholds;

            var sorted = new float[count];
            for (int site = 0; site < count; site++)
                sorted[site] = noise[site];
            Array.Sort(sorted);

            // Boundaries may fall only between runs of equal values, never inside a run.
            var boundaries = new List<int> { 0 };
            for (int i = 1; i < count; i++)
                if (sorted[i] != sorted[i - 1])
                    boundaries.Add(i);
            boundaries.Add(count);

            // Scaling first avoids overflow when several finite shares are near double.MaxValue.
            double total = 0d;
            foreach (var target in targets)
                total += target.AreaShare / maximumShare;

            double cumulative = 0d;
            for (int i = 0; i < thresholds.Length; i++)
            {
                cumulative += targets[i].AreaShare / maximumShare;
                double desiredCount = System.Math.Min(count, cumulative / total * count);
                int upper = boundaries.BinarySearch((int)System.Math.Ceiling(desiredCount));
                if (upper < 0)
                    upper = ~upper;
                int boundary = boundaries[upper];
                if (upper > 0 && desiredCount - boundaries[upper - 1] <= boundary - desiredCount)
                    boundary = boundaries[upper - 1];
                thresholds[i] = boundary == count ? float.PositiveInfinity : sorted[boundary];
            }
            return thresholds;
        }
    }
}
