using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides scaling helpers for read-only lists.
    /// </summary>
    public static class IReadOnlyListExtensions
    {
        /// <summary>
        /// Scales every item in the list.
        /// </summary>
        /// <typeparam name="T">The scalable item type.</typeparam>
        /// <param name="items">The items to scale.</param>
        /// <param name="scale">The scale factor.</param>
        /// <returns>A new mutable list of scaled items owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        public static List<T> Scale<T>(this IReadOnlyList<T> items, VectorXY scale)
            where T : IScalable<T>
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Scalable items collection must not be empty.", nameof(items));

            var res = new List<T>(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is null)
                    throw new ArgumentException("Scalable items collection cannot contain null elements.", nameof(items));

                var scaled = item.Scale(scale);

                res.Add(scaled);
            }

            return res;
        }
    }
}
