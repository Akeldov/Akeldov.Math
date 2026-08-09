using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides centroid and nearest-item helpers for positioned collections.
    /// </summary>
    public static class PositionedCollectionExtensions
    {
        /// <summary>
        /// Returns the centroid, computed as the arithmetic mean of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The centroid of item positions.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static PointXY GetCentroid<TItem>(this IReadOnlyList<TItem> items)
            where TItem : IHasPosition2D
        {
            ValidateItems(items);

            float sumX = 0f;
            float sumY = 0f;
            for (int k = 0; k < items.Count; k++)
            {
                sumX += items[k].Position.X;
                sumY += items[k].Position.Y;
            }

            return new PointXY(sumX / items.Count, sumY / items.Count);
        }

        /// <summary>
        /// Returns the centroid, computed as the arithmetic mean of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The centroid of item positions.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static PointXY GetCentroid<TItem>(this TItem[] items)
            where TItem : IHasPosition2D
        {
            ValidateItems(items);

            float sumX = 0f;
            float sumY = 0f;
            for (int k = 0; k < items.Length; k++)
            {
                sumX += items[k].Position.X;
                sumY += items[k].Position.Y;
            }

            return new PointXY(sumX / items.Length, sumY / items.Length);
        }

        /// <summary>
        /// Returns the item closest to the centroid of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The item closest to the centroid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestToCentroid<TItem>(this IReadOnlyList<TItem> items)
            where TItem : IHasPosition2D
        {
            var centroid = items.GetCentroid();

            var closestItem = items[0];
            var minDist = closestItem.Position.Distance(centroid);
            for (int k = 1; k < items.Count; k++)
            {
                var item = items[k];
                var distance = item.Position.Distance(centroid);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestItem = item;
                }
            }
            return closestItem;
        }

        /// <summary>
        /// Returns the item closest to the centroid of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The item closest to the centroid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestToCentroid<TItem>(this TItem[] items)
            where TItem : IHasPosition2D
        {
            var centroid = items.GetCentroid();

            var closestItem = items[0];
            var minDist = closestItem.Position.Distance(centroid);
            for (int k = 1; k < items.Length; k++)
            {
                var item = items[k];
                var distance = item.Position.Distance(centroid);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestItem = item;
                }
            }
            return closestItem;
        }

        /// <summary>
        /// Returns the item closest to the specified point.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <param name="point">The target point.</param>
        /// <returns>The item closest to the target point.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestTo<TItem>(this TItem[] items, PointXY point)
            where TItem : IHasPosition2D
        {
            ValidateItems(items);

            var closestItem = items[0];
            var minDist = closestItem.Position.Distance(point);
            for (int k = 1; k < items.Length; k++)
            {
                var item = items[k];
                var distance = item.Position.Distance(point);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestItem = item;
                }
            }
            return closestItem;
        }

        /// <summary>
        /// Returns the item closest to the specified point.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <param name="point">The target point.</param>
        /// <returns>The item closest to the target point.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestTo<TItem>(this IReadOnlyList<TItem> items, PointXY point)
            where TItem : IHasPosition2D
        {
            ValidateItems(items);

            var closestItem = items[0];
            var minDist = closestItem.Position.Distance(point);
            for (int k = 1; k < items.Count; k++)
            {
                var item = items[k];
                var distance = item.Position.Distance(point);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestItem = item;
                }
            }
            return closestItem;
        }

        private static void ValidateItems<TItem>(IReadOnlyList<TItem> items)
            where TItem : IHasPosition2D
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Positioned items collection must not be empty.", nameof(items));

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is null)
                    throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));
            }
        }
    }
}
