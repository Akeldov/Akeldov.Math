using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Applies radius-one dilation over each cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// when the source cell itself or at least one existing neighbor is <see langword="true"/>.
        /// Neighbors outside the map domain are ignored, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Dilate(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            FillDilatedValues(map, values);
            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Applies radius-one dilation over each spatial cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// A result cell is <see langword="true"/> when the source cell itself or at least one existing
        /// neighbor is <see langword="true"/>. Neighbors outside the map domain are ignored, and the
        /// source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Dilate(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            FillDilatedValues(map, values);
            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Applies radius-one erosion over each cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// only when the source cell itself and every existing neighbor are <see langword="true"/>.
        /// Neighbors outside the map domain are ignored, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Erode(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            FillErodedValues(map, values);
            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Applies radius-one erosion over each spatial cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// A result cell is <see langword="true"/> only when the source cell itself and every existing
        /// neighbor are <see langword="true"/>. Neighbors outside the map domain are ignored, and the
        /// source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Erode(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            FillErodedValues(map, values);
            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Applies radius-one morphological opening: erosion followed by dilation.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. Both passes use the source domain,
        /// ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Open(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                FillErodedValues(map, scratch);
                FillDilatedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Applies radius-one morphological opening to a spatial map: erosion followed by dilation.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// Both passes use the source domain, ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Open(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                FillErodedValues(map, scratch);
                FillDilatedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Applies radius-one morphological closing: dilation followed by erosion.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. Both passes use the source domain,
        /// ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Close(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                FillDilatedValues(map, scratch);
                FillErodedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Applies radius-one morphological closing to a spatial map: dilation followed by erosion.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// Both passes use the source domain, ignore neighbors outside it, and leave the source map unmodified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Close(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            bool[] scratch = ArrayPool<bool>.Shared.Rent(map.Topology.Count);
            try
            {
                FillDilatedValues(map, scratch);
                FillErodedValues(scratch, map.Topology, values);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(scratch, clearArray: false);
            }

            return new SpatialBoolHexMap(map.Geometry, values);
        }

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

        private static void FillDilatedValues(IHexMap<bool> source, bool[] destination)
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
                    if (source[flatIndex])
                    {
                        destination[flatIndex] = true;
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
                        if (source[neighborIndex])
                        {
                            value = true;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }

        private static void FillDilatedValues(bool[] source, HexMapTopology topology, bool[] destination)
        {
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
                    if (source[flatIndex])
                    {
                        destination[flatIndex] = true;
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
                        if (source[neighborIndex])
                        {
                            value = true;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }

        private static void FillErodedValues(IHexMap<bool> source, bool[] destination)
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
                    bool value = true;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        int neighborX = x + offsets[offsetIndex].X;
                        int neighborY = y + offsets[offsetIndex].Y;
                        if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
                            continue;

                        int neighborIndex = neighborY * width + neighborX;
                        if (!source[neighborIndex])
                        {
                            value = false;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }

        private static void FillErodedValues(bool[] source, HexMapTopology topology, bool[] destination)
        {
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
                    bool value = true;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        int neighborX = x + offsets[offsetIndex].X;
                        int neighborY = y + offsets[offsetIndex].Y;
                        if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
                            continue;

                        int neighborIndex = neighborY * width + neighborX;
                        if (!source[neighborIndex])
                        {
                            value = false;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
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
