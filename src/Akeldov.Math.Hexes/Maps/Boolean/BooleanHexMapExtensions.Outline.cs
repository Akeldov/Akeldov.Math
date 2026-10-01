using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Extracts the radius-one inner boundary of a Boolean map.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// only when its source cell is <see langword="true"/> and at least one existing edge-adjacent
        /// neighbor is <see langword="false"/>. Neighbors outside the map domain are ignored, and the
        /// source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Outline(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            FillOutlineValues(map, values);
            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Extracts the radius-one inner boundary of a spatial Boolean map.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// A result cell is <see langword="true"/> only when its source cell is <see langword="true"/>
        /// and at least one existing edge-adjacent neighbor is <see langword="false"/>. Neighbors outside
        /// the map domain are ignored, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Outline(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            FillOutlineValues(map, values);
            return new SpatialBoolHexMap(map.Geometry, values);
        }

        private static void FillOutlineValues(IHexMap<bool> source, bool[] destination)
        {
            HexMapTopology topology = source.Topology;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                for (int x = 0; x < width; x++)
                {
                    int flatIndex = rowStart + x;
                    if (!source[flatIndex])
                    {
                        destination[flatIndex] = false;
                        continue;
                    }

                    bool axisIsEven = ((parityUsesY ? y : x) & 1) == 0;
                    VectorXYInt[] offsets = axisIsEven ? evenOffsets : oddOffsets;
                    bool value = false;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        int neighborX = x + offsets[offsetIndex].X;
                        int neighborY = y + offsets[offsetIndex].Y;
                        if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
                            continue;

                        int neighborIndex = neighborY * width + neighborX;
                        if (!source[neighborIndex])
                        {
                            value = true;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }
    }
}
