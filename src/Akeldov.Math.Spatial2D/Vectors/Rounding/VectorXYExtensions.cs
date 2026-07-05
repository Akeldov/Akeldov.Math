namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Rounds each component of each vector in an array to the specified number of fractional digits.
        /// </summary>
        /// <param name="vectors">The vectors to round.</param>
        /// <param name="digits">The number of fractional digits in the return values.</param>
        /// <returns>A new array containing rounded vectors.</returns>
        public static VectorXY[] Round(this VectorXY[] vectors, int digits)
        {
            var res = new VectorXY[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                res[i] = vectors[i].Round(digits);
            }
            return res;
        }

        /// <summary>
        /// Rounds each component of a vector to the specified number of fractional digits.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="digits">The number of fractional digits in the return values.</param>
        /// <returns>The rounded vector.</returns>
        public static VectorXY Round(this VectorXY vector, int digits)
        {
            return new VectorXY(System.MathF.Round(vector.X, digits), System.MathF.Round(vector.Y, digits));
        }
    }
}
