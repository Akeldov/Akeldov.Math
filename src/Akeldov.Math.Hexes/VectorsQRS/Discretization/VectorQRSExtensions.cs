using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Rounds fractional QRS coordinates to the nearest valid integer hex index.
        /// </summary>
        /// <param name="axialPoint">The fractional QRS coordinates to discretize.</param>
        /// <param name="layout">The layout whose orientation determines tie correction.</param>
        /// <returns>The nearest integer QRS index.</returns>
        public static VectorQRSInt ToQRSIndex(this VectorQRS axialPoint, Layout layout)
        {
            if (float.IsNaN(axialPoint.Q) || float.IsInfinity(axialPoint.Q) ||
                float.IsNaN(axialPoint.R) || float.IsInfinity(axialPoint.R))
                throw new ArgumentOutOfRangeException(nameof(axialPoint), axialPoint, "Axial point components must be finite.");

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return ToQRSIndexPointyTop(axialPoint);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return ToQRSIndexFlatTop(axialPoint);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorQRSInt ToQRSIndexPointyTop(VectorQRS axialPoint)
        {
            float s = -axialPoint.Q - axialPoint.R;

            if (float.IsNaN(s) || float.IsInfinity(s))
                throw new ArgumentOutOfRangeException(nameof(axialPoint), axialPoint, "Derived axial component must be finite.");

            int qInt = RoundToInt32(axialPoint.Q, axialPoint);
            int rInt = RoundToInt32(axialPoint.R, axialPoint);
            int sInt = RoundToInt32(s, axialPoint);

            float qDiff = MathF.Abs(qInt - axialPoint.Q);
            float rDiff = MathF.Abs(rInt - axialPoint.R);
            float sDiff = MathF.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = GetCheckedNegatedSum(rInt, sInt, axialPoint);
            else if (rDiff > sDiff)
                rInt = GetCheckedNegatedSum(qInt, sInt, axialPoint);

            return new VectorQRSInt(qInt, rInt);
        }

        private static VectorQRSInt ToQRSIndexFlatTop(VectorQRS axialPoint)
        {
            float s = -axialPoint.Q - axialPoint.R;

            if (float.IsNaN(s) || float.IsInfinity(s))
                throw new ArgumentOutOfRangeException(nameof(axialPoint), axialPoint, "Derived axial component must be finite.");

            int qInt = RoundToInt32(axialPoint.Q, axialPoint);
            int rInt = RoundToInt32(axialPoint.R, axialPoint);
            int sInt = RoundToInt32(s, axialPoint);

            float qDiff = MathF.Abs(qInt - axialPoint.Q);
            float rDiff = MathF.Abs(rInt - axialPoint.R);
            float sDiff = MathF.Abs(sInt - s);

            if (rDiff > qDiff && rDiff > sDiff)
                rInt = GetCheckedNegatedSum(qInt, sInt, axialPoint);
            else if (qDiff > sDiff)
                qInt = GetCheckedNegatedSum(rInt, sInt, axialPoint);

            return new VectorQRSInt(qInt, rInt);
        }

        private static int RoundToInt32(float value, VectorQRS axialPoint)
        {
            double rounded = MathF.Round(value);
            if (rounded < int.MinValue || rounded > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(axialPoint), axialPoint, "Rounded axial components must fit in Int32.");

            return (int)rounded;
        }

        private static int GetCheckedNegatedSum(int left, int right, VectorQRS axialPoint)
        {
            long value = -(long)left - right;
            if (value < int.MinValue || value > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(axialPoint), axialPoint, "Rounded axial components must fit in Int32.");

            return (int)value;
        }
    }
}
