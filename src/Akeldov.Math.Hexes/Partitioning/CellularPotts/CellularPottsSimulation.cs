using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    internal sealed class CellularPottsSimulation
    {
        private const int MaxNeighbors = 6;

        private readonly HexMapTopology _topology;
        private readonly int[] _owners;
        private readonly int[] _cellIds;
        private readonly int[]? _regions;
        private readonly int[] _activeSites;
        private readonly int[] _cellAreas;
        private readonly double[] _targetAreas;
        private readonly double _temperature;
        private readonly double _boundaryEnergy;
        private readonly double _areaPenalty;
        private readonly bool _preserveConnectivity;
        private readonly int[] _neighbors;
        private readonly byte[] _neighborCounts;
        private readonly byte[] _neighborPresence;

        internal CellularPottsSimulation(HexMapTopology topology, int[] assignments, int[] ids,
            IReadOnlyDictionary<int, float>? targetAreas, CellularPottsParameters parameters, int[]? regions = null)
        {
            _topology = topology;
            _owners = assignments;
            _cellIds = ids;
            _regions = regions;
            // Widen coefficients before any arithmetic, including multiplication by integer edge counts.
            _temperature = parameters.Temperature;
            _boundaryEnergy = parameters.BoundaryEnergy;
            _areaPenalty = parameters.AreaPenalty;
            _preserveConnectivity = parameters.Connectivity == CellularPottsConnectivity.Preserve;
            var activeSites = new List<int>();
            for (int site = 0; site < assignments.Length; site++)
                if (assignments[site] >= 0)
                    activeSites.Add(site);
            _activeSites = activeSites.ToArray();

            _cellAreas = new int[ids.Length];
            _targetAreas = new double[ids.Length];
            (_neighbors, _neighborCounts, _neighborPresence) = BuildNeighbors(topology, assignments);

            foreach (int owner in assignments)
                if (owner >= 0)
                    _cellAreas[owner]++;
            for (int cell = 0; cell < ids.Length; cell++)
                _targetAreas[cell] = targetAreas != null && targetAreas.TryGetValue(ids[cell], out float area)
                    ? (double)area : _cellAreas[cell];
        }

        internal bool HasConnectedCells()
        {
            var visited = new bool[_owners.Length];
            var queue = new int[_owners.Length];
            for (int seed = 0; seed < _owners.Length; seed++)
            {
                if (visited[seed] || _owners[seed] < 0)
                    continue;
                int cell = _owners[seed];
                int head = 0;
                int tail = 1;
                queue[0] = seed;
                visited[seed] = true;
                while (head < tail)
                {
                    int current = queue[head++];
                    int offset = current * MaxNeighbors;
                    for (int i = 0; i < _neighborCounts[current]; i++)
                    {
                        int neighbor = _neighbors[offset + i];
                        if (visited[neighbor] || _owners[neighbor] != cell)
                            continue;
                        visited[neighbor] = true;
                        queue[tail++] = neighbor;
                    }
                }
                if (tail != _cellAreas[cell])
                    return false;
            }
            return true;
        }

        internal void Run(int steps, Random random)
        {
            if (_activeSites.Length == 0)
                return;

            for (int step = 0; step < steps; step++)
            {
                for (int attempt = 0; attempt < _activeSites.Length; attempt++)
                {
                    int site = _activeSites[random.Next(_activeSites.Length)];
                    int count = _neighborCounts[site];
                    if (count == 0)
                        continue;

                    int neighbor = _neighbors[site * MaxNeighbors + random.Next(count)];
                    // Regions restrict copies only; energy and connectivity still use the entire source map.
                    if (_regions != null && _regions[site] != _regions[neighbor])
                        continue;
                    TryTransfer(site, _owners[neighbor], random);
                }
            }
        }

        private void TryTransfer(int site, int receiver, Random random)
        {
            int donor = _owners[site];
            if (donor == receiver)
                return;
            if (_preserveConnectivity && !CanRemoveSite(site, donor))
                return;

            double energyChange = CalculateEnergyChange(site, donor, receiver);
            if (energyChange > 0d && (_temperature == 0d ||
                random.NextDouble() >= System.Math.Exp(-energyChange / _temperature)))
                return;

            // The receiving cell is attached through the sampled neighbor. Only the donor can split.
            _owners[site] = receiver;
            _cellAreas[donor]--;
            _cellAreas[receiver]++;
        }

        private double CalculateEnergyChange(int site, int donor, int receiver)
        {
            int edgeChange = 0;
            int offset = site * MaxNeighbors;
            for (int i = 0; i < _neighborCounts[site]; i++)
            {
                int neighborOwner = _owners[_neighbors[offset + i]];
                if (neighborOwner == donor)
                    edgeChange++;
                else if (neighborOwner == receiver)
                    edgeChange--;
            }

            double donorDeviation = _cellAreas[donor] - _targetAreas[donor];
            double receiverDeviation = _cellAreas[receiver] - _targetAreas[receiver];
            // Expanding the two changed squares avoids subtracting large total energies.
            double areaChange = 2d * (receiverDeviation - donorDeviation + 1d);
            return _boundaryEnergy * edgeChange + _areaPenalty * areaChange;
        }

        private bool CanRemoveSite(int site, int cell)
        {
            if (_cellAreas[cell] == 1)
                return false;

            int mask = 0;
            int presence = _neighborPresence[site];
            int offset = site * MaxNeighbors;
            for (int direction = 0; direction < MaxNeighbors; direction++)
            {
                if ((presence & (1 << direction)) == 0)
                    continue;
                int neighbor = _neighbors[offset++];
                if (_owners[neighbor] == cell)
                    mask |= 1 << direction;
            }

            if (mask == 0)
                return false;
            // Consecutive directions share edges. A single cyclic run connects every donor neighbor.
            int preceding = ((mask << 1) | (mask >> (MaxNeighbors - 1))) & 0x3f;
            int runStarts = mask & ~preceding;
            return (runStarts & (runStarts - 1)) == 0;
        }

        internal HexPartitionMap<HexPartitionCell> CreatePartition()
            => new HexPartitionMap<HexPartitionCell>(_topology, CreateCells());

        internal PartialHexPartitionMap<HexPartitionCell> CreatePartialPartition()
            => new PartialHexPartitionMap<HexPartitionCell>(_topology, CreateCells());

        internal HexPartitionCell[] CreateCells()
        {
            var groups = new List<VectorXYInt>[_cellIds.Length];
            for (int cell = 0; cell < groups.Length; cell++)
                groups[cell] = new List<VectorXYInt>(_cellAreas[cell]);
            int width = _topology.Resolution.X;
            for (int site = 0; site < _owners.Length; site++)
                if (_owners[site] >= 0)
                    groups[_owners[site]].Add(new VectorXYInt(site % width, site / width));

            var cells = new HexPartitionCell[_cellIds.Length];
            for (int cell = 0; cell < cells.Length; cell++)
                cells[cell] = new HexPartitionCell(_cellIds[cell], groups[cell]);
            return cells;
        }

        private static (int[] Neighbors, byte[] Counts, byte[] Presence) BuildNeighbors(
            HexMapTopology topology, int[] assignments)
        {
            int count = topology.Count;
            var neighbors = new int[checked(count * MaxNeighbors)];
            var counts = new byte[count];
            var presence = new byte[count];
            var evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            var oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            bool rowParity = topology.Layout.IsPointyTop();
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int site = y * width + x;
                    if (assignments[site] < 0)
                        continue;
                    var offsets = (((rowParity ? y : x) & 1) == 0) ? evenOffsets : oddOffsets;
                    for (int direction = 0; direction < MaxNeighbors; direction++)
                    {
                        int nx = x + offsets[direction].X;
                        int ny = y + offsets[direction].Y;
                        if ((uint)nx < (uint)width && (uint)ny < (uint)height)
                        {
                            int neighbor = ny * width + nx;
                            if (assignments[neighbor] < 0)
                                continue;
                            neighbors[site * MaxNeighbors + counts[site]++] = neighbor;
                            presence[site] |= (byte)(1 << direction);
                        }
                    }
                }
            }
            return (neighbors, counts, presence);
        }
    }
}
