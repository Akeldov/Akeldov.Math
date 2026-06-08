using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    internal static partial class BoolMaskExtensions
    {
        public static bool[,] Clone(bool[,] mask)
        {
            var qDimension = mask.GetLength(0);
            var rDimension = mask.GetLength(1);
            var res = new bool[qDimension, rDimension];
            for (int i = 0; i < qDimension; i++)
            {
                for (int j = 0; j < rDimension; j++)
                {
                    res[i, j] = mask[i, j];
                }
            }
            return res;
        }

        public static int PositiveSize(this bool[,] mask)
        {
            var res = 0;

            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(1); j++)
                {
                    if (mask[i, j])
                        res++;
                }
            }

            return res;
        }

        public static bool[,] GetExtended(this bool[,] mask)
        {
            var res = new bool[mask.GetLength(0) + 2, mask.GetLength(1) + 2];

            for (int i = 0; i < res.GetLength(0); i++)
            {
                for (int j = 0; j < res.GetLength(1); j++)
                {
                    var isWithinSourceMatrix =
                        i > 0 &&
                        i < res.GetLength(0) - 1 &&
                        j > 0 &&
                        j < res.GetLength(1) - 1;

                    if (isWithinSourceMatrix)
                    {
                        if (mask[i - 1, j - 1])
                        {
                            res[i, j] = true;
                        }
                        else
                        {
                            res[i, j] = false;
                        }
                    }
                    else
                    {
                        var sextuple = mask.GetSextuple(i - 1, j - 1);

                        var hasTrue = false;
                        for (int k = 0; k < sextuple.Count; k++)
                        {
                            if (sextuple[k])
                            {
                                hasTrue = true;
                                break;
                            }
                        }

                        res[i, j] = hasTrue;
                    }
                }
            }

            return res;
        }

        public static bool[,] GetContour(this bool[,] mask)
        {
            var sourceMaskWidth = mask.GetLength(0);
            var sourceMaskHeight = mask.GetLength(1);
            var targetMaskWidth = sourceMaskWidth + 2;
            var targetMaskHeight = sourceMaskHeight + 2;
            var res = new bool[targetMaskWidth, targetMaskHeight];

            for (int i = 0; i < sourceMaskWidth; i++)
            {
                for (int j = 0; j < sourceMaskHeight; j++)
                {
                    var value = mask[i, j];

                    if (value == true)
                    {
                        if (i + 1 < sourceMaskWidth)
                            res[i + 2, j + 1] = !mask[i + 1, j];
                        else
                            res[i + 2, j + 1] = true;

                        if (i - 1 >= 0)
                            res[i, j + 1] = !mask[i - 1, j];
                        else
                            res[i, j + 1] = true;

                        if (j + 1 < sourceMaskHeight)
                            res[i + 1, j + 2] = !mask[i, j + 1];
                        else
                            res[i + 1, j + 2] = true;

                        if (j - 1 >= 0)
                            res[i + 1, j] = !mask[i, j - 1];
                        else
                            res[i + 1, j] = true;

                        if (i - 1 >= 0 && j + 1 < sourceMaskHeight)
                            res[i, j + 2] = !mask[i - 1, j + 1];
                        else
                            res[i, j + 2] = true;

                        if (i + 1 < sourceMaskWidth && j - 1 >= 0)
                            res[i + 2, j] = !mask[i + 1, j - 1];
                        else
                            res[i + 2, j] = true;

                    }
                }
            }

            return res;
        }

        internal static List<bool> GetSextuple(this bool[,] mask, int i, int j)
        {
            var res = new List<bool>(6);

            if (i + 1 >= 0 && i + 1 < mask.GetLength(0) && j >= 0 && j < mask.GetLength(1))
                res.Add(mask[i + 1, j]);

            if (i - 1 >= 0 && i - 1 < mask.GetLength(0) && j >= 0 && j < mask.GetLength(1))
                res.Add(mask[i - 1, j]);

            if (i >= 0 && i < mask.GetLength(0) && j + 1 >= 0 && j + 1 < mask.GetLength(1))
                res.Add(mask[i, j + 1]);

            if (i >= 0 && i < mask.GetLength(0) && j - 1 >= 0 && j - 1 < mask.GetLength(1))
                res.Add(mask[i, j - 1]);

            if (i - 1 >= 0 && i - 1 < mask.GetLength(0) && j + 1 >= 0 && j + 1 < mask.GetLength(1))
                res.Add(mask[i - 1, j + 1]);

            if (i + 1 >= 0 && i + 1 < mask.GetLength(0) && j - 1 >= 0 && j - 1 < mask.GetLength(1))
                res.Add(mask[i + 1, j - 1]);

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAnyTrue(this List<bool> sextuple)
        {
            for (int i = 0; i < sextuple.Count; i++)
            {
                if (sextuple[i])
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsBorder(this bool[,] mask, int i, int j)
        {
            if (mask[i, j] == false)
            {
                if (mask.GetSextuple(i - 1, j - 1).HasAnyTrue())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
