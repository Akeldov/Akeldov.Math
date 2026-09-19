using System;
using System.Collections.Generic;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Spatial3D namespace.
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130
{
    /// <summary>
    /// Provides centroid and nearest-item helpers for three-dimensional positioned collections.
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
        public static PointXYZ GetCentroid<TItem>(this IReadOnlyList<TItem> items)
            where TItem : IHasPosition3D
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Positioned items collection must not be empty.", nameof(items));

            double sumX = 0d;
            double sumY = 0d;
            double sumZ = 0d;
            for (int k = 0; k < items.Count; k++)
            {
                var item = items[k];
                if (item is null)
                    throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

                var position = item.Position;
                sumX += position.X;
                sumY += position.Y;
                sumZ += position.Z;
            }

            return new PointXYZ((float)(sumX / items.Count), (float)(sumY / items.Count), (float)(sumZ / items.Count));
        }

        /// <summary>
        /// Returns the centroid, computed as the arithmetic mean of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The centroid of item positions.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static PointXYZ GetCentroid<TItem>(this TItem[] items)
            where TItem : IHasPosition3D
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Length == 0)
                throw new ArgumentException("Positioned items collection must not be empty.", nameof(items));

            double sumX = 0d;
            double sumY = 0d;
            double sumZ = 0d;
            for (int k = 0; k < items.Length; k++)
            {
                var item = items[k];
                if (item is null)
                    throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

                var position = item.Position;
                sumX += position.X;
                sumY += position.Y;
                sumZ += position.Z;
            }

            return new PointXYZ((float)(sumX / items.Length), (float)(sumY / items.Length), (float)(sumZ / items.Length));
        }

        /// <summary>
        /// Returns the item closest to the centroid of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The item closest to the centroid; the first such item if distances are equal.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestToCentroid<TItem>(this IReadOnlyList<TItem> items)
            where TItem : IHasPosition3D => items.GetClosestTo(items.GetCentroid());

        /// <summary>
        /// Returns the item closest to the centroid of item positions.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <returns>The item closest to the centroid; the first such item if distances are equal.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestToCentroid<TItem>(this TItem[] items)
            where TItem : IHasPosition3D => items.GetClosestTo(items.GetCentroid());

        /// <summary>
        /// Returns the item closest to the specified point.
        /// </summary>
        /// <typeparam name="TItem">The positioned item type.</typeparam>
        /// <param name="items">The positioned items.</param>
        /// <param name="point">The target point.</param>
        /// <returns>The item closest to the target point; the first such item if distances are equal.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestTo<TItem>(this TItem[] items, PointXYZ point)
            where TItem : IHasPosition3D
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Length == 0)
                throw new ArgumentException("Positioned items collection must not be empty.", nameof(items));

            var closestItem = items[0];
            if (closestItem is null)
                throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

            var minDistance = GetSquaredDistance(closestItem.Position, point);
            for (int k = 1; k < items.Length; k++)
            {
                var item = items[k];
                if (item is null)
                    throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

                var distance = GetSquaredDistance(item.Position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
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
        /// <returns>The item closest to the target point; the first such item if distances are equal.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty or contains null elements.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
        public static TItem GetClosestTo<TItem>(this IReadOnlyList<TItem> items, PointXYZ point)
            where TItem : IHasPosition3D
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Positioned items collection must not be empty.", nameof(items));

            var closestItem = items[0];
            if (closestItem is null)
                throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

            var minDistance = GetSquaredDistance(closestItem.Position, point);
            for (int k = 1; k < items.Count; k++)
            {
                var item = items[k];
                if (item is null)
                    throw new ArgumentException("Positioned items collection cannot contain null elements.", nameof(items));

                var distance = GetSquaredDistance(item.Position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestItem = item;
                }
            }

            return closestItem;
        }

        private static double GetSquaredDistance(PointXYZ left, PointXYZ right)
        {
            double dx = (double)left.X - right.X;
            double dy = (double)left.Y - right.Y;
            double dz = (double)left.Z - right.Z;

            return dx * dx + dy * dy + dz * dz;
        }
    }
}
