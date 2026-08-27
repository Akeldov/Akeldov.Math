using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides Gaussian blur operations for floating-point hex maps.
    /// </summary>
    public static class FloatHexMapBlurExtensions
    {
        /// <summary>
        /// Applies a Gaussian blur using a kernel truncated at three standard deviations.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in distances between the
        /// centers of edge-adjacent hexes.
        /// </param>
        /// <returns>
        /// A new mutable hex map owned by the caller. The source map is not modified. At map
        /// boundaries, weights are normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive.
        /// </exception>
        public static FloatHexMap GaussianBlur(this IHexMap<float> map, float sigma)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            int maximumRadius = GetMaximumUsefulRadius(map.Topology);
            int radius = (int)System.Math.Min(System.Math.Ceiling(3d * sigma), maximumRadius);
            return new FloatHexMap(map.Topology, CreateBlurredValues(map, sigma, radius));
        }

        /// <summary>
        /// Applies a Gaussian blur to a spatial floating-point hex map using a kernel truncated at
        /// three standard deviations.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in distances between the
        /// centers of edge-adjacent hexes.
        /// </param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. The result preserves
        /// the source geometry, and the source map is not modified. At map boundaries, weights are
        /// normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive.
        /// </exception>
        public static SpatialFloatHexMap GaussianBlur(this SpatialFloatHexMap map, float sigma)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            int maximumRadius = GetMaximumUsefulRadius(map.Topology);
            int radius = (int)System.Math.Min(System.Math.Ceiling(3d * sigma), maximumRadius);
            return new SpatialFloatHexMap(map.Geometry, CreateBlurredValues(map, sigma, radius));
        }

        /// <summary>
        /// Applies a Gaussian blur using an explicitly truncated kernel.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in distances between the
        /// centers of edge-adjacent hexes.
        /// </param>
        /// <param name="radius">
        /// The non-negative kernel radius in hex steps. A radius of zero returns an independent
        /// copy of the source values.
        /// </param>
        /// <returns>
        /// A new mutable hex map owned by the caller. The source map is not modified. At map
        /// boundaries, weights are normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive, or when
        /// <paramref name="radius"/> is negative.
        /// </exception>
        public static FloatHexMap GaussianBlur(this IHexMap<float> map, float sigma, int radius)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Gaussian kernel radius must be non-negative.");

            radius = System.Math.Min(radius, GetMaximumUsefulRadius(map.Topology));
            return new FloatHexMap(map.Topology, CreateBlurredValues(map, sigma, radius));
        }

        /// <summary>
        /// Applies a Gaussian blur to a spatial floating-point hex map using an explicitly
        /// truncated kernel.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in distances between the
        /// centers of edge-adjacent hexes.
        /// </param>
        /// <param name="radius">
        /// The non-negative kernel radius in hex steps. A radius of zero returns an independent
        /// spatial copy of the source values.
        /// </param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. The result preserves
        /// the source geometry, and the source map is not modified. At map boundaries, weights are
        /// normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive, or when
        /// <paramref name="radius"/> is negative.
        /// </exception>
        public static SpatialFloatHexMap GaussianBlur(this SpatialFloatHexMap map, float sigma, int radius)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Gaussian kernel radius must be non-negative.");

            radius = System.Math.Min(radius, GetMaximumUsefulRadius(map.Topology));
            return new SpatialFloatHexMap(map.Geometry, CreateBlurredValues(map, sigma, radius));
        }

        private static float[] CreateBlurredValues(IHexMap<float> map, float sigma, int radius)
        {
            HexMapTopology topology = map.Topology;
            var values = new float[topology.Count];
            if (values.Length == 0)
                return values;

            GaussianKernelEntry[] kernel = CreateKernel(sigma, radius);
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            int outputIndex = 0;

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                VectorQRSInt center = new VectorXYInt(x, y).ToQRSIndex(topology.Layout);
                double weightedValue = 0d;
                double weightSum = 0d;

                for (int kernelIndex = 0; kernelIndex < kernel.Length; kernelIndex++)
                {
                    GaussianKernelEntry entry = kernel[kernelIndex];
                    long sourceQ = (long)center.Q + entry.Q;
                    long sourceR = (long)center.R + entry.R;

                    if (!TryGetFlatIndex(sourceQ, sourceR, topology, out int sourceIndex))
                        continue;

                    weightedValue += map[sourceIndex] * entry.Weight;
                    weightSum += entry.Weight;
                }

                values[outputIndex++] = (float)(weightedValue / weightSum);
            }

            return values;
        }

        private static GaussianKernelEntry[] CreateKernel(float sigma, int radius)
        {
            var entries = new List<GaussianKernelEntry>();
            double varianceFactor = 2d * sigma * sigma;

            for (long q = -radius; q <= radius; q++)
            {
                long minimumR = System.Math.Max(-radius, -q - radius);
                long maximumR = System.Math.Min(radius, -q + radius);

                for (long r = minimumR; r <= maximumR; r++)
                {
                    double distanceSquared = (double)q * q + (double)q * r + (double)r * r;
                    double weight = System.Math.Exp(-distanceSquared / varianceFactor);
                    entries.Add(new GaussianKernelEntry((int)q, (int)r, weight));
                }
            }

            return entries.ToArray();
        }

        private static int GetMaximumUsefulRadius(HexMapTopology topology)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            if (width == 0 || height == 0)
                return 0;

            var corners = new[]
            {
                new VectorXYInt(0, 0).ToQRSIndex(topology.Layout),
                new VectorXYInt(width - 1, 0).ToQRSIndex(topology.Layout),
                new VectorXYInt(0, height - 1).ToQRSIndex(topology.Layout),
                new VectorXYInt(width - 1, height - 1).ToQRSIndex(topology.Layout)
            };

            long maximumDistance = 0;
            for (int leftIndex = 0; leftIndex < corners.Length - 1; leftIndex++)
            for (int rightIndex = leftIndex + 1; rightIndex < corners.Length; rightIndex++)
            {
                VectorQRSInt left = corners[leftIndex];
                VectorQRSInt right = corners[rightIndex];
                long qDistance = System.Math.Abs((long)left.Q - right.Q);
                long rDistance = System.Math.Abs((long)left.R - right.R);
                long sDistance = System.Math.Abs((long)left.S - right.S);
                long distance = System.Math.Max(qDistance, System.Math.Max(rDistance, sDistance));
                maximumDistance = System.Math.Max(maximumDistance, distance);
            }

            return checked((int)maximumDistance);
        }

        private static bool TryGetFlatIndex(
            long q,
            long r,
            HexMapTopology topology,
            out int flatIndex)
        {
            long x;
            long y;

            switch (topology.Layout)
            {
                case Layout.OddR:
                    x = q + ((r - (r & 1L)) / 2L);
                    y = r;
                    break;
                case Layout.EvenR:
                    x = q + ((r + (r & 1L)) / 2L);
                    y = r;
                    break;
                case Layout.OddQ:
                    x = q;
                    y = r + ((q - (q & 1L)) / 2L);
                    break;
                case Layout.EvenQ:
                    x = q;
                    y = r + ((q + (q & 1L)) / 2L);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(topology), topology, "Hex map topology layout is not supported.");
            }

            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            if ((ulong)x >= (ulong)width || (ulong)y >= (ulong)height)
            {
                flatIndex = default;
                return false;
            }

            flatIndex = checked((int)(y * width + x));
            return true;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
        private readonly struct GaussianKernelEntry
        {
            public GaussianKernelEntry(int q, int r, double weight)
            {
                Q = q;
                R = r;
                Weight = weight;
            }

            public int Q { get; }

            public int R { get; }

            public double Weight { get; }
        }
    }
}
