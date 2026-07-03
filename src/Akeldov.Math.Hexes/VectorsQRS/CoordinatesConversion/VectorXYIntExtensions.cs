using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="index">The index value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRSInt ToQRSIndex(this VectorXYInt index, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorQRSInt(index.X - ((index.Y - (index.Y & 1)) / 2), index.Y);
                case Layout.EvenR:
                    return new VectorQRSInt(index.X - ((index.Y + (index.Y & 1)) / 2), index.Y);
                case Layout.OddQ:
                    return new VectorQRSInt(index.X, index.Y - ((index.X - (index.X & 1)) / 2));
                case Layout.EvenQ:
                    return new VectorQRSInt(index.X, index.Y - ((index.X + (index.X & 1)) / 2));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
