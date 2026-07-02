using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    public static partial class SixfoldAngleExtensions
    {
        private static float[] Sinuses = new float[]
        {
            HexAngleConstants.Sin0Deg,
            HexAngleConstants.Sin60Deg,
            HexAngleConstants.Sin120Deg,
            HexAngleConstants.Sin180Deg,
            HexAngleConstants.Sin240Deg,
            HexAngleConstants.Sin300Deg
        };

        private static float[] Cosines = new float[]
        {
            HexAngleConstants.Cos0Deg,
            HexAngleConstants.Cos60Deg,
            HexAngleConstants.Cos120Deg,
            HexAngleConstants.Cos180Deg,
            HexAngleConstants.Cos240Deg,
            HexAngleConstants.Cos300Deg
        };

        private static float[] Radians = new float[]
        {
            HexAngleConstants.Rad0Deg,
            HexAngleConstants.Rad60Deg,
            HexAngleConstants.Rad120Deg,
            HexAngleConstants.Rad180Deg,
            HexAngleConstants.Rad240Deg,
            HexAngleConstants.Rad300Deg
        };

        private static float[] Degrees = new float[]
        {
            0f,
            60f,
            120f,
            180f,
            240f,
            300f
        };

        private static SixfoldAngle[] Negates = new SixfoldAngle[]
        {
            SixfoldAngle.Deg0,
            SixfoldAngle.Deg300,
            SixfoldAngle.Deg240,
            SixfoldAngle.Deg180,
            SixfoldAngle.Deg120,
            SixfoldAngle.Deg60
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(this SixfoldAngle angle)
        {
            var index = (int)angle;
            if ((uint)index >= 6u)
                throw new System.ArgumentOutOfRangeException(
                    nameof(angle),
                    angle,
                    "The angle must be a defined sixfold angle.");

            return Sinuses[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(this SixfoldAngle angle)
        {
            var index = (int)angle;
            if ((uint)index >= 6u)
                throw new System.ArgumentOutOfRangeException(
                    nameof(angle),
                    angle,
                    "The angle must be a defined sixfold angle.");

            return Cosines[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsFloatRadians(this SixfoldAngle angle)
        {
            var index = (int)angle;
            if ((uint)index >= 6u)
                throw new System.ArgumentOutOfRangeException(
                    nameof(angle),
                    angle,
                    "The angle must be a defined sixfold angle.");

            return Radians[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsFloatDegrees(this SixfoldAngle angle)
        {
            var index = (int)angle;
            if ((uint)index >= 6u)
                throw new System.ArgumentOutOfRangeException(
                    nameof(angle),
                    angle,
                    "The angle must be a defined sixfold angle.");

            return Degrees[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Negate(this SixfoldAngle angle)
        {
            var index = (int)angle;
            if ((uint)index >= 6u)
                throw new System.ArgumentOutOfRangeException(
                    nameof(angle),
                    angle,
                    "The angle must be a defined sixfold angle.");

            return Negates[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Add180(this SixfoldAngle angle)
        {
            switch(angle)
            {
                case SixfoldAngle.Deg0:
                    return SixfoldAngle.Deg180;
                case SixfoldAngle.Deg60:
                    return SixfoldAngle.Deg240;
                case SixfoldAngle.Deg120:
                    return SixfoldAngle.Deg300;
                case SixfoldAngle.Deg180:
                    return SixfoldAngle.Deg0;
                case SixfoldAngle.Deg240:
                    return SixfoldAngle.Deg60;
                case SixfoldAngle.Deg300:
                    return SixfoldAngle.Deg120;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Add120(this SixfoldAngle angle)
        {
            switch (angle)
            {
                case SixfoldAngle.Deg0:
                    return SixfoldAngle.Deg120;
                case SixfoldAngle.Deg60:
                    return SixfoldAngle.Deg180;
                case SixfoldAngle.Deg120:
                    return SixfoldAngle.Deg240;
                case SixfoldAngle.Deg180:
                    return SixfoldAngle.Deg300;
                case SixfoldAngle.Deg240:
                    return SixfoldAngle.Deg0;
                case SixfoldAngle.Deg300:
                    return SixfoldAngle.Deg60;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Add60(this SixfoldAngle angle)
        {
            switch (angle)
            {
                case SixfoldAngle.Deg0:
                    return SixfoldAngle.Deg60;
                case SixfoldAngle.Deg60:
                    return SixfoldAngle.Deg120;
                case SixfoldAngle.Deg120:
                    return SixfoldAngle.Deg180;
                case SixfoldAngle.Deg180:
                    return SixfoldAngle.Deg240;
                case SixfoldAngle.Deg240:
                    return SixfoldAngle.Deg300;
                case SixfoldAngle.Deg300:
                    return SixfoldAngle.Deg0;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Add240(this SixfoldAngle angle)
        {
            switch (angle)
            {
                case SixfoldAngle.Deg0:
                    return SixfoldAngle.Deg240;
                case SixfoldAngle.Deg60:
                    return SixfoldAngle.Deg300;
                case SixfoldAngle.Deg120:
                    return SixfoldAngle.Deg0;
                case SixfoldAngle.Deg180:
                    return SixfoldAngle.Deg60;
                case SixfoldAngle.Deg240:
                    return SixfoldAngle.Deg120;
                case SixfoldAngle.Deg300:
                    return SixfoldAngle.Deg180;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Add300(this SixfoldAngle angle)
        {
            switch (angle)
            {
                case SixfoldAngle.Deg0:
                    return SixfoldAngle.Deg300;
                case SixfoldAngle.Deg60:
                    return SixfoldAngle.Deg0;
                case SixfoldAngle.Deg120:
                    return SixfoldAngle.Deg60;
                case SixfoldAngle.Deg180:
                    return SixfoldAngle.Deg120;
                case SixfoldAngle.Deg240:
                    return SixfoldAngle.Deg180;
                case SixfoldAngle.Deg300:
                    return SixfoldAngle.Deg240;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
