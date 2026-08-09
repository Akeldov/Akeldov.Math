using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides aggregation helpers for vector sequences.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns the sum of all vectors in a sequence.
        /// </summary>
        /// <param name="vectors">The vectors to sum.</param>
        /// <returns>The sum of all vectors, or <see cref="VectorXY.Zero"/> when the sequence is empty.</returns>
        public static VectorXY Sum(this IEnumerable<VectorXY> vectors)
        {
            var res = VectorXY.Zero;

            if (vectors is IReadOnlyList<VectorXY> vectorList)
            {
                for (int i = 0; i < vectorList.Count; i++)
                    res = res + vectorList[i];

                return res;
            }

            foreach (var vector in vectors)
            {
                res = res + vector;
            }

            return res;
        }

        /// <summary>
        /// Returns the arithmetic mean of all vectors in a sequence.
        /// </summary>
        /// <param name="vectors">The vectors to average.</param>
        /// <returns>The arithmetic mean of all vectors.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="vectors"/> is empty.</exception>
        public static VectorXY Average(this IEnumerable<VectorXY> vectors)
        {
            var res = VectorXY.Zero;
            var count = 0;

            if (vectors is IReadOnlyList<VectorXY> vectorList)
            {
                for (int i = 0; i < vectorList.Count; i++)
                    res = res + vectorList[i];

                count = vectorList.Count;
            }
            else
            {
                foreach (var vector in vectors)
                {
                    res = res + vector;
                    count++;
                }
            }

            if (count == 0)
                throw new InvalidOperationException("Sequence contains no elements.");

            return res / count;
        }
    }
}
