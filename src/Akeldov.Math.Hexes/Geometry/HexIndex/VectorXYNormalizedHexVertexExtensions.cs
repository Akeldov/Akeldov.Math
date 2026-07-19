using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        internal static readonly VectorXY[] RowLayoutNormalizedHexVertices =
        {
            new VectorXY(Constants.Cos30Deg, Constants.Sin30Deg),
            new VectorXY(Constants.Cos90Deg, Constants.Sin90Deg),
            new VectorXY(Constants.Cos150Deg, Constants.Sin150Deg),
            new VectorXY(Constants.Cos210Deg, Constants.Sin210Deg),
            new VectorXY(Constants.Cos270Deg, Constants.Sin270Deg),
            new VectorXY(Constants.Cos330Deg, Constants.Sin330Deg),
        };

        internal static readonly VectorXY[] ColumnLayoutNormalizedHexVertices =
        {
            new VectorXY(Constants.Cos0Deg, Constants.Sin0Deg),
            new VectorXY(Constants.Cos60Deg, Constants.Sin60Deg),
            new VectorXY(Constants.Cos120Deg, Constants.Sin120Deg),
            new VectorXY(Constants.Cos180Deg, Constants.Sin180Deg),
            new VectorXY(Constants.Cos240Deg, Constants.Sin240Deg),
            new VectorXY(Constants.Cos300Deg, Constants.Sin300Deg),
        };

        /// <summary>
        /// Gets normalized vertex offsets for a unit-radius hex in the specified layout.
        /// </summary>
        /// <param name="layout">The layout that determines vertex orientation.</param>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetNormalizedHexVertices(Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return (VectorXY[])RowLayoutNormalizedHexVertices.Clone();
                case Layout.OddQ:
                case Layout.EvenQ:
                    return (VectorXY[])ColumnLayoutNormalizedHexVertices.Clone();
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

    }
}
