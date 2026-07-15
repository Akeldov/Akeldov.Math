using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class PointXYExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="hexRadius">The hexRadius value.</param>
        /// <param name="hexFieldOrigin">The hexFieldOrigin value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToXYIndex(this PointXY point, float hexRadius, VectorXY hexFieldOrigin, Layout layout)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            if (!hexFieldOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexFieldOrigin), hexFieldOrigin, "Hex field origin components must be finite.");

            float shiftedX = point.X - hexFieldOrigin.X;
            float shiftedY = point.Y - hexFieldOrigin.Y;
            float q;
            float r;
            bool isPointyTop;

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    q = 0.5773502588f * shiftedX - 0.3333333333f * shiftedY;
                    r = 0.6666666666f * shiftedY;
                    isPointyTop = true;
                    break;
                case Layout.OddQ:
                case Layout.EvenQ:
                    q = 0.6666666666f * shiftedX;
                    r = 0.5773502588f * shiftedY - 0.3333333333f * shiftedX;
                    isPointyTop = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }

            float invertedHexRadius = 1f / hexRadius;
            q *= invertedHexRadius;
            r *= invertedHexRadius;

            float s = -q - r;
            int qInt = (int)MathF.Round(q);
            int rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);
            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (isPointyTop)
            {
                if (qDiff > rDiff && qDiff > sDiff)
                    qInt = -rInt - sInt;
                else if (rDiff > sDiff)
                    rInt = -qInt - sInt;
            }
            else
            {
                if (rDiff > qDiff && rDiff > sDiff)
                    rInt = -qInt - sInt;
                else if (qDiff > sDiff)
                    qInt = -rInt - sInt;
            }

            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXYInt(qInt + ((rInt - (rInt & 1)) / 2), rInt);
                case Layout.EvenR:
                    return new VectorXYInt(qInt + ((rInt + (rInt & 1)) / 2), rInt);
                case Layout.OddQ:
                    return new VectorXYInt(qInt, rInt + ((qInt - (qInt & 1)) / 2));
                case Layout.EvenQ:
                    return new VectorXYInt(qInt, rInt + ((qInt + (qInt & 1)) / 2));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        /// <summary>
        /// Converts the point to an odd-row offset hex index.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <param name="hexRadius">The positive hexagon radius.</param>
        /// <param name="hexFieldOrigin">The origin of the hex field.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToOddRXYIndex(this PointXY point, float hexRadius, VectorXY hexFieldOrigin)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");
            if (!hexFieldOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexFieldOrigin), hexFieldOrigin, "Hex field origin components must be finite.");

            float invertedRadius = 1f / hexRadius;
            float shiftedX = point.X - hexFieldOrigin.X;
            float shiftedY = point.Y - hexFieldOrigin.Y;
            float q = (0.5773502588f * shiftedX - 0.3333333333f * shiftedY) * invertedRadius;
            float r = 0.6666666666f * shiftedY * invertedRadius;
            float s = -q - r;
            int qInt = (int)MathF.Round(q);
            int rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);
            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = -rInt - sInt;
            else if (rDiff > sDiff)
                rInt = -qInt - sInt;

            return new VectorXYInt(qInt + ((rInt - (rInt & 1)) / 2), rInt);
        }

        /// <summary>
        /// Converts the point to an even-row offset hex index.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <param name="hexRadius">The positive hexagon radius.</param>
        /// <param name="hexFieldOrigin">The origin of the hex field.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToEvenRXYIndex(this PointXY point, float hexRadius, VectorXY hexFieldOrigin)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");
            if (!hexFieldOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexFieldOrigin), hexFieldOrigin, "Hex field origin components must be finite.");

            float invertedRadius = 1f / hexRadius;
            float shiftedX = point.X - hexFieldOrigin.X;
            float shiftedY = point.Y - hexFieldOrigin.Y;
            float q = (0.5773502588f * shiftedX - 0.3333333333f * shiftedY) * invertedRadius;
            float r = 0.6666666666f * shiftedY * invertedRadius;
            float s = -q - r;
            int qInt = (int)MathF.Round(q);
            int rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);
            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
                qInt = -rInt - sInt;
            else if (rDiff > sDiff)
                rInt = -qInt - sInt;

            return new VectorXYInt(qInt + ((rInt + (rInt & 1)) / 2), rInt);
        }

        /// <summary>
        /// Converts the point to an odd-column offset hex index.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <param name="hexRadius">The positive hexagon radius.</param>
        /// <param name="hexFieldOrigin">The origin of the hex field.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToOddQXYIndex(this PointXY point, float hexRadius, VectorXY hexFieldOrigin)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");
            if (!hexFieldOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexFieldOrigin), hexFieldOrigin, "Hex field origin components must be finite.");

            float invertedRadius = 1f / hexRadius;
            float shiftedX = point.X - hexFieldOrigin.X;
            float shiftedY = point.Y - hexFieldOrigin.Y;
            float q = 0.6666666666f * shiftedX * invertedRadius;
            float r = (0.5773502588f * shiftedY - 0.3333333333f * shiftedX) * invertedRadius;
            float s = -q - r;
            int qInt = (int)MathF.Round(q);
            int rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);
            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (rDiff > qDiff && rDiff > sDiff)
                rInt = -qInt - sInt;
            else if (qDiff > sDiff)
                qInt = -rInt - sInt;

            return new VectorXYInt(qInt, rInt + ((qInt - (qInt & 1)) / 2));
        }

        /// <summary>
        /// Converts the point to an even-column offset hex index.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <param name="hexRadius">The positive hexagon radius.</param>
        /// <param name="hexFieldOrigin">The origin of the hex field.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToEvenQXYIndex(this PointXY point, float hexRadius, VectorXY hexFieldOrigin)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");
            if (!hexFieldOrigin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexFieldOrigin), hexFieldOrigin, "Hex field origin components must be finite.");

            float invertedRadius = 1f / hexRadius;
            float shiftedX = point.X - hexFieldOrigin.X;
            float shiftedY = point.Y - hexFieldOrigin.Y;
            float q = 0.6666666666f * shiftedX * invertedRadius;
            float r = (0.5773502588f * shiftedY - 0.3333333333f * shiftedX) * invertedRadius;
            float s = -q - r;
            int qInt = (int)MathF.Round(q);
            int rInt = (int)MathF.Round(r);
            int sInt = (int)MathF.Round(s);
            float qDiff = MathF.Abs(qInt - q);
            float rDiff = MathF.Abs(rInt - r);
            float sDiff = MathF.Abs(sInt - s);

            if (rDiff > qDiff && rDiff > sDiff)
                rInt = -qInt - sInt;
            else if (qDiff > sDiff)
                qInt = -rInt - sInt;

            return new VectorXYInt(qInt, rInt + ((qInt + (qInt & 1)) / 2));
        }
    }
}
