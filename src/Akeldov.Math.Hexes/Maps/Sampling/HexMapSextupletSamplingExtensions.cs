using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides six-neighbor sampling operations for hex maps.
    /// </summary>
    public static class HexMapSextupletSamplingExtensions
    {
        /// <summary>
        /// Samples the six values adjacent to a center cell in hex-edge order.
        /// </summary>
        /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="index">The center cell whose neighbors are sampled. The center value is not included.</param>
        /// <returns>
        /// A sextuplet whose <c>Adjacent0</c> through <c>Adjacent5</c> values correspond exactly to
        /// <see cref="HexEdge.Edge0"/> through <see cref="HexEdge.Edge5"/>.
        /// </returns>
        /// <remarks>
        /// The center and all six adjacent cells must be inside the map. All neighbor coordinates
        /// are validated before any cell value is read.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is outside the map or any adjacent cell is outside the map.
        /// </exception>
        public static Sextuplet<TValue> SampleSextuplet<TValue>(
            this IHexMap<TValue> map,
            VectorXYInt index)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            if ((uint)index.X >= (uint)width || (uint)index.Y >= (uint)height)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Center hex index must be inside the map.");

            bool axisIsEven = ((topology.Layout.IsPointyTop() ? index.Y : index.X) & 1) == 0;
            VectorXYInt[] offsets = axisIsEven.GetSharedRelativeOffsets(topology.Layout);

            int adjacent0X = index.X + offsets[0].X;
            int adjacent0Y = index.Y + offsets[0].Y;
            int adjacent1X = index.X + offsets[1].X;
            int adjacent1Y = index.Y + offsets[1].Y;
            int adjacent2X = index.X + offsets[2].X;
            int adjacent2Y = index.Y + offsets[2].Y;
            int adjacent3X = index.X + offsets[3].X;
            int adjacent3Y = index.Y + offsets[3].Y;
            int adjacent4X = index.X + offsets[4].X;
            int adjacent4Y = index.Y + offsets[4].Y;
            int adjacent5X = index.X + offsets[5].X;
            int adjacent5Y = index.Y + offsets[5].Y;

            bool hasAdjacent0 = (uint)adjacent0X < (uint)width && (uint)adjacent0Y < (uint)height;
            bool hasAdjacent1 = (uint)adjacent1X < (uint)width && (uint)adjacent1Y < (uint)height;
            bool hasAdjacent2 = (uint)adjacent2X < (uint)width && (uint)adjacent2Y < (uint)height;
            bool hasAdjacent3 = (uint)adjacent3X < (uint)width && (uint)adjacent3Y < (uint)height;
            bool hasAdjacent4 = (uint)adjacent4X < (uint)width && (uint)adjacent4Y < (uint)height;
            bool hasAdjacent5 = (uint)adjacent5X < (uint)width && (uint)adjacent5Y < (uint)height;

            if (!hasAdjacent0 ||
                !hasAdjacent1 ||
                !hasAdjacent2 ||
                !hasAdjacent3 ||
                !hasAdjacent4 ||
                !hasAdjacent5)
                throw new ArgumentOutOfRangeException(nameof(index), index, "All six adjacent hexes must be inside the map.");

            return new Sextuplet<TValue>(
                map[adjacent0Y * width + adjacent0X],
                map[adjacent1Y * width + adjacent1X],
                map[adjacent2Y * width + adjacent2X],
                map[adjacent3Y * width + adjacent3X],
                map[adjacent4Y * width + adjacent4X],
                map[adjacent5Y * width + adjacent5X]);
        }

        /// <summary>
        /// Samples the existing values adjacent to a center cell in hex-edge order.
        /// </summary>
        /// <typeparam name="TValue">The type of value stored in the map.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="index">The center cell whose neighbors are sampled. The center value is not included.</param>
        /// <returns>
        /// A partial sextuplet whose <c>Adjacent0</c> through <c>Adjacent5</c> positions correspond
        /// exactly to <see cref="HexEdge.Edge0"/> through <see cref="HexEdge.Edge5"/>. An adjacent
        /// position is marked present when its cell is inside the map; missing positions store
        /// <see langword="default"/> values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is outside the map.
        /// </exception>
        public static PartialSextuplet<TValue> SamplePartialSextuplet<TValue>(
            this IHexMap<TValue> map,
            VectorXYInt index)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            if ((uint)index.X >= (uint)width || (uint)index.Y >= (uint)height)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Center hex index must be inside the map.");

            bool axisIsEven = ((topology.Layout.IsPointyTop() ? index.Y : index.X) & 1) == 0;
            VectorXYInt[] offsets = axisIsEven.GetSharedRelativeOffsets(topology.Layout);

            SextupletPresenceFlags presence = SextupletPresenceFlags.None;

            TValue adjacent0 = SampleAdjacent(
                map, index, offsets[0], width, height, SextupletPresenceFlags.Adjacent0, ref presence);
            TValue adjacent1 = SampleAdjacent(
                map, index, offsets[1], width, height, SextupletPresenceFlags.Adjacent1, ref presence);
            TValue adjacent2 = SampleAdjacent(
                map, index, offsets[2], width, height, SextupletPresenceFlags.Adjacent2, ref presence);
            TValue adjacent3 = SampleAdjacent(
                map, index, offsets[3], width, height, SextupletPresenceFlags.Adjacent3, ref presence);
            TValue adjacent4 = SampleAdjacent(
                map, index, offsets[4], width, height, SextupletPresenceFlags.Adjacent4, ref presence);
            TValue adjacent5 = SampleAdjacent(
                map, index, offsets[5], width, height, SextupletPresenceFlags.Adjacent5, ref presence);

            return new PartialSextuplet<TValue>(
                adjacent0,
                adjacent1,
                adjacent2,
                adjacent3,
                adjacent4,
                adjacent5,
                presence);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue SampleAdjacent<TValue>(
            IHexMap<TValue> map,
            VectorXYInt center,
            VectorXYInt offset,
            int width,
            int height,
            SextupletPresenceFlags flag,
            ref SextupletPresenceFlags presence)
        {
            int adjacentX = center.X + offset.X;
            int adjacentY = center.Y + offset.Y;
            if ((uint)adjacentX >= (uint)width || (uint)adjacentY >= (uint)height)
                return default!;

            presence |= flag;
            return map[adjacentY * width + adjacentX];
        }
    }
}
