using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides coherent-noise generation extensions for hex-map topologies.
    /// </summary>
    public static class HexMapTopologyPerlinNoiseExtensions
    {
        /// <summary>
        /// Creates a floating-point map by sampling fractal Perlin noise at the center of every hex.
        /// The hex radius (center to vertex) is assumed to be 1.
        /// </summary>
        /// <param name="topology">The topology of the generated map.</param>
        /// <param name="seed">The deterministic seed used to select lattice gradients.</param>
        /// <param name="scale">
        /// The base feature scale in unit-hex-radius coordinate-space units. Larger values produce
        /// broader features.
        /// </param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">
        /// The amplitude multiplier between successive octaves, in the inclusive range [0, 1].
        /// </param>
        /// <param name="lacunarity">The positive frequency multiplier between successive octaves.</param>
        /// <param name="offset">
        /// The sample-space offset in unit-hex-radius coordinate-space units.
        /// </param>
        /// <returns>
        /// A new mutable floating-point map owned by the caller. Every value is in the inclusive
        /// range [0, 1].
        /// </returns>
        /// <remarks>
        /// A topology has no spatial size or position. Sampling uses a hex radius of 1 and the layout's
        /// default zero-hex origin. Both <paramref name="scale"/> and <paramref name="offset"/> are
        /// expressed in this unit-radius coordinate system.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="scale"/> is not finite and positive,
        /// <paramref name="octaves"/> is not positive, <paramref name="persistence"/> is not finite
        /// or lies outside [0, 1], <paramref name="lacunarity"/> is not finite and positive, or
        /// <paramref name="offset"/> contains a non-finite component.
        /// </exception>
        public static FloatHexMap CreatePerlinNoise(
            this HexMapTopology topology,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            var geometry = new HexMapGeometry(topology, 1f);
            var noise = geometry.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);
            return new FloatHexMap(topology, noise.BackingValues);
        }
    }
}
