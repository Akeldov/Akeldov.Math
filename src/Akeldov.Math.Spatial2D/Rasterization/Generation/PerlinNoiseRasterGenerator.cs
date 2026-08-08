namespace Akeldov.Math.Spatial2D.Rasterization
{
    internal static class PerlinNoiseRasterGenerator
    {
        public static float[] CreateValues(
            VectorXYInt resolution,
            double firstX,
            double firstY,
            double stepX,
            double stepY,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity,
            VectorXY offset)
        {
            var values = new float[checked(resolution.X * resolution.Y)];
            int valueIndex = 0;

            for (int y = 0; y < resolution.Y; y++)
            {
                double pointY = firstY + y * stepY;

                for (int x = 0; x < resolution.X; x++)
                {
                    double pointX = firstX + x * stepX;
                    values[valueIndex++] = SampleFractalNoise(
                        pointX,
                        pointY,
                        seed,
                        scale,
                        octaves,
                        persistence,
                        lacunarity,
                        offset);
                }
            }

            return values;
        }

        private static float SampleFractalNoise(
            double pointX,
            double pointY,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity,
            VectorXY offset)
        {
            double frequency = 1d / scale;
            double amplitude = 1d;
            double value = 0d;
            double amplitudeSum = 0d;

            for (int octave = 0; octave < octaves; octave++)
            {
                double sampleX = (pointX + offset.X) * frequency;
                double sampleY = (pointY + offset.Y) * frequency;

                value += PerlinNoise2D.Sample(sampleX, sampleY, seed) * amplitude;
                amplitudeSum += amplitude;
                amplitude *= persistence;

                if (amplitude == 0d)
                    break;

                frequency *= lacunarity;
            }

            double normalized = 0.5d + 0.5d * value / amplitudeSum;
            return (float)System.Math.Min(System.Math.Max(normalized, 0d), 1d);
        }
    }
}
