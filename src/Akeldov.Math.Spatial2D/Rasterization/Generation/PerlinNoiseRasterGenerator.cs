using Akeldov.Math.Spatial2D.Fields;

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
            PerlinNoiseField field)
        {
            var values = new float[checked(resolution.X * resolution.Y)];
            int valueIndex = 0;

            for (int y = 0; y < resolution.Y; y++)
            {
                double pointY = firstY + y * stepY;

                for (int x = 0; x < resolution.X; x++)
                {
                    double pointX = firstX + x * stepX;
                    values[valueIndex++] = field.Sample(pointX, pointY);
                }
            }

            return values;
        }
    }
}
