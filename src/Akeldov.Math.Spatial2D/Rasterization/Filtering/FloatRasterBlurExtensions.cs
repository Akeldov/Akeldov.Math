using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Provides Gaussian blur operations for floating-point rasters.
    /// </summary>
    public static class FloatRasterBlurExtensions
    {
        /// <summary>
        /// Applies a Gaussian blur using a kernel truncated at three standard deviations.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in raster cells.
        /// </param>
        /// <returns>
        /// A new mutable raster owned by the caller. The source raster is not modified. At raster
        /// boundaries, weights are normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="raster"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive.
        /// </exception>
        public static Raster<float> GaussianBlur(this IRaster<float> raster, float sigma)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            VectorXYInt resolution = raster.Resolution;
            int maximumRadius = System.Math.Max(resolution.X - 1, resolution.Y - 1);
            int radius = (int)System.Math.Min(System.Math.Ceiling(3d * sigma), maximumRadius);
            return GaussianBlurCore(raster, sigma, radius);
        }

        /// <summary>
        /// Applies a Gaussian blur using an explicitly truncated kernel.
        /// </summary>
        /// <param name="raster">The source raster.</param>
        /// <param name="sigma">
        /// The positive finite Gaussian standard deviation, measured in raster cells.
        /// </param>
        /// <param name="radius">
        /// The non-negative kernel radius in cells. A radius of zero returns an independent copy
        /// of the source values.
        /// </param>
        /// <returns>
        /// A new mutable raster owned by the caller. The source raster is not modified. At raster
        /// boundaries, weights are normalized over the source cells that are present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="raster"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sigma"/> is not finite and positive, or when
        /// <paramref name="radius"/> is negative.
        /// </exception>
        public static Raster<float> GaussianBlur(this IRaster<float> raster, float sigma, int radius)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));

            if (float.IsNaN(sigma) || float.IsInfinity(sigma) || sigma <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Gaussian sigma must be finite and positive.");

            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Gaussian kernel radius must be non-negative.");

            VectorXYInt resolution = raster.Resolution;
            int maximumRadius = System.Math.Max(resolution.X - 1, resolution.Y - 1);
            radius = System.Math.Min(radius, maximumRadius);
            return GaussianBlurCore(raster, sigma, radius);
        }

        private static Raster<float> GaussianBlurCore(IRaster<float> raster, float sigma, int radius)
        {
            VectorXYInt resolution = raster.Resolution;
            int width = resolution.X;
            int height = resolution.Y;
            var horizontalValues = new double[checked(width * height)];
            var values = new float[horizontalValues.Length];
            double[] weights = CreateWeights(sigma, radius);

            int outputIndex = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int minimumX = System.Math.Max(0, x - radius);
                int maximumX = (int)System.Math.Min((long)width - 1L, (long)x + radius);
                double weightedValue = 0d;
                double weightSum = 0d;

                for (int sourceX = minimumX; sourceX <= maximumX; sourceX++)
                {
                    double weight = weights[System.Math.Abs(sourceX - x)];
                    weightedValue += raster[sourceX, y] * weight;
                    weightSum += weight;
                }

                horizontalValues[outputIndex++] = weightedValue / weightSum;
            }

            outputIndex = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int minimumY = System.Math.Max(0, y - radius);
                int maximumY = (int)System.Math.Min((long)height - 1L, (long)y + radius);
                double weightedValue = 0d;
                double weightSum = 0d;

                for (int sourceY = minimumY; sourceY <= maximumY; sourceY++)
                {
                    double weight = weights[System.Math.Abs(sourceY - y)];
                    weightedValue += horizontalValues[sourceY * width + x] * weight;
                    weightSum += weight;
                }

                values[outputIndex++] = (float)(weightedValue / weightSum);
            }

            return new Raster<float>(resolution, values);
        }

        private static double[] CreateWeights(float sigma, int radius)
        {
            var weights = new double[radius + 1];
            double varianceFactor = 2d * sigma * sigma;

            for (int distance = 0; distance < weights.Length; distance++)
                weights[distance] = System.Math.Exp(-(double)distance * distance / varianceFactor);

            return weights;
        }
    }
}
