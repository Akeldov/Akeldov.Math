using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class SixfoldAngleExtensions
    {
        private static float[] Sinuses = new float[]
        {
            Constants.Sin0Deg,
            Constants.Sin60Deg,
            Constants.Sin120Deg,
            Constants.Sin180Deg,
            Constants.Sin240Deg,
            Constants.Sin300Deg
        };

        private static float[] Cosinuses = new float[]
        {
            Constants.Cos0Deg,
            Constants.Cos60Deg,
            Constants.Cos120Deg,
            Constants.Cos180Deg,
            Constants.Cos240Deg,
            Constants.Cos300Deg
        };

        private static float[] Radians = new float[]
        {
            Constants.Rad0Deg,
            Constants.Rad60Deg,
            Constants.Rad120Deg,
            Constants.Rad180Deg,
            Constants.Rad240Deg,
            Constants.Rad300Deg
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
        public static float Sin(this SixfoldAngle angle) => Sinuses[(int)angle];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(this SixfoldAngle angle) => Cosinuses[(int)angle];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsFloatRadians(this SixfoldAngle angle) => Radians[(int)angle];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsFloatDegrees(this SixfoldAngle angle) => Degrees[(int)angle];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle Negate(this SixfoldAngle angle) => Negates[(int)angle];

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
